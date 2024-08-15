import { CourtDocumentType, DocumentData } from "@/types/shared";
import { splunkLog } from "@/utils/utils";
import base64url from "base64url";
import { v4 as uuidv4 } from 'uuid';

export default {
  openDocumentsPdf(documentType: CourtDocumentType, documentData: DocumentData): void {
    const fileName = this.generateFileName(documentType, documentData).replace(/\//g,"_");
    const isCriminal = documentType == CourtDocumentType.Criminal;
    const documentId = documentData.documentId
      ? base64url(documentData.documentId)
      : documentData.documentId;
    const correlationId = uuidv4();
        
    switch (documentType) {
      case CourtDocumentType.CSR:
        window.open(
          `${process.env.BASE_URL}api/files/civil/court-summary-report/${documentData.appearanceId}/${encodeURIComponent(fileName)}?vcCivilFileId=${documentData.fileId}`
        );
        break;
      case CourtDocumentType.ROP:
        window.open(
          `${process.env.BASE_URL}api/files/criminal/record-of-proceedings/${documentData.partId}/${encodeURIComponent(fileName)}?profSequenceNumber=${documentData.profSeqNo}&courtLevelCode=${documentData.courtLevel}&courtClassCode=${documentData.courtClass}`
        );
        break;
      default:
        this.openRequestedTab(
          `${process.env.BASE_URL}api/files/document/${documentId}/${encodeURIComponent(fileName)}?isCriminal=${isCriminal}&fileId=${documentData.fileId}&CorrelationId=${correlationId}`,
          correlationId
        );
        break;
    }
  },

  generateFileName(documentType: CourtDocumentType, documentData: DocumentData): string {
    const locationAbbreviation = (documentData.location.match(/[A-Z]/g) || []).join("");
    switch (documentType) {
      case CourtDocumentType.Civil:
        return `${locationAbbreviation}-${documentData.courtLevel}-${documentData.fileNumberText}-${documentData.documentDescription}-${documentData.dateFiled}-${documentData.documentId}.pdf`;
      case CourtDocumentType.ProvidedCivil:
        return `${locationAbbreviation}-${documentData.courtLevel}-${documentData.fileNumberText}-${documentData.documentDescription}-${documentData.appearanceDate}-${documentData.partyName}.pdf`;
      case CourtDocumentType.CSR:
        return `${locationAbbreviation}-${documentData.courtLevel}-${documentData.fileNumberText}-${documentData.documentDescription}-${documentData.appearanceDate}.pdf`;
      case CourtDocumentType.Criminal:
        return `${locationAbbreviation}-${documentData.courtLevel}-${documentData.courtClass}-${documentData.fileNumberText}-${documentData.documentDescription}-${documentData.dateFiled}-${documentData.documentId}.pdf`;
      case CourtDocumentType.ROP:
        return `${locationAbbreviation}-${documentData.courtLevel}-${documentData.courtClass}-${documentData.fileNumberText}-${documentData.documentDescription}-${documentData.partId}.pdf`;
      case CourtDocumentType.CivilZip:
        return `${locationAbbreviation}-${documentData.courtLevel}-${documentData.fileNumberText}-documents.zip`
      case CourtDocumentType.CriminalZip:
        return `${locationAbbreviation}-${documentData.courtLevel}-${documentData.courtClass}-${documentData.fileNumberText}-documents.zip`
      default:
        throw Error(`No file structure for type: ${documentType}`);
    }
  },

  openRequestedTab(url, correlationId) {
    const start = new Date();
    const startStr = start.toLocaleString("en-US", {
      timeZone: "America/Vancouver"
    });
    const startMsg = `Request Tracking - Frontend request to API - CorrelationId: ${correlationId} Start time: ${startStr}`;
    //console.log(startMsg);
    splunkLog(startMsg);

    const windowObjectReference = window.open(url);
    if (windowObjectReference !== null) {
      const end = new Date();
      const endStr = start.toLocaleString("en-US", {
        timeZone: "America/Vancouver"
      });

      const duration = (end.getTime() - start.getTime()) / 1000;
      const endMsg = `Request Tracking - API response received - CorrelationId: ${correlationId} End time: ${endStr} Duration: ${duration}s`;
      
      // eslint-disable-next-line
      windowObjectReference.onload = (event) => {
        if (windowObjectReference.document.readyState === "complete") {
          //console.log(endMsg);
          splunkLog(endMsg);
        }
      };
    }
  },

  submitUploadRequest(url, parent) {
    parent.$http.get(url).then(
      (response) => {
        const blob = response.data;
        const transferId = blob.transferId;
        const url = `api/files/status?transferId=${transferId}`;

        parent.$http.get(url).then(
          (response) => {
            const blob = response.data;
            blob.variant = "success";
            parent.progressValues.push(blob);
            parent.$bvModal.show('progress-modal');
          },
          (err) => {
            parent.$bvToast.toast(`Error - ${err.url} - ${err.status} - ${err.statusText}`, {
              title: "An error has occured.",
              variant: "danger",
              autoHideDelay: 10000,
            });
            console.log(err);
          });
      },
      (err) => {
        parent.$bvToast.toast(`Error - ${err.url} - ${err.status} - ${err.statusText}`, {
          title: "An error has occured.",
          variant: "danger",
          autoHideDelay: 10000,
        });
        console.log(err);
      }
    ) 
  },

  // Method to start polling for progress values
  startPolling(parent) {
    parent.pollingInterval = setInterval(() => {
      parent.progressValues.every((transfer, index) => {
        if (this.downloadIsComplete(parent)) {
          parent.$bvModal.hide('progress-modal');
          parent.stopPolling();
          return false;
        } else {
          const url = `api/files/status?transferId=${transfer.transferId}`;

          if (transfer.percentTransfered < 100) {
            parent.$http.get(url).then(
              (response) => {
                  const blob = response.data;
                  if (blob.error == true && parent.progressValues[index].error === false) {
                    parent.$bvToast.toast(`Error - ${blob.fileName} - ${blob.lastErrorMessage}`, {
                      title: "An error has occured.",
                      variant: "danger",
                      autoHideDelay: 10000,
                    });
                    parent.progressValues[index].error = true;
                    parent.progressValues[index].percentTransfered = 100;
                    parent.progressValues[index].variant = "danger";
                  } else if (blob.error === false) {
                    parent.progressValues[index].error = false;
                    parent.progressValues[index].percentTransfered = blob.percentTransfered;
                    parent.progressValues[index].variant = "success";
                  }
                  parent.progressValues[index].fileName = blob.fileName;
              },
              (err) => {
                parent.$bvToast.toast(`Error - ${err.url} - ${err.status} - ${err.statusText}`, {
                  title: "An error has occured.",
                  variant: "danger",
                  autoHideDelay: 10000,
                });
                console.log(err);
              });
            }
            return true;
          }
      });
    }, 1000);
  },

  cancelDownload(parent) {
    parent.$bvModal.hide('progress-modal');
    parent.cleanUp();

    const options = {
      responseType: "blob",
      headers: {
        "Content-Type": "application/json",
      },
    };
    const url = "api/files/terminate";
    parent.cancelPreDownloadInfo.transferIds = parent.progressValues.map(transfer => transfer.transferId);

    parent.$http.post(url, parent.cancelPreDownloadInfo, options).then(
      () => {
        parent.$bvToast.toast("The download request was cancelled.", {
          title: "Download Cancelled",
          variant: "success",
          autoHideDelay: 10000,
        });
        parent.progressValues = [];
      },
      (err) => {
        parent.$bvToast.toast(`Error - ${err.url} - ${err.status} - ${err.statusText}`, {
          title: "An error has occured.",
          variant: "danger",
          autoHideDelay: 10000,
        });
        console.log(err);
      }
    );
  },

  stopPolling(parent) {
    parent.cleanUp(parent);

    parent.$bvToast.toast("All files have been transferred to OneDrive.", {
      title: "Download Complete",
      variant: "success",
      autoHideDelay: 10000,
    });
      
    parent.progressValues = [];

  },

   downloadIsComplete(parent) {
    let complete = true;
    parent.progressValues.forEach(progress => {
      if (progress.percentTransfered != 100 || progress.error === true) {
        complete = false;
      }
    });
    
    return complete;
  }
};
