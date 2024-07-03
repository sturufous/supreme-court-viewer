<template>
  <div>
    <b-card v-if="isMounted" no-body>
      <div>
        <b-row>
          <h3 class="ml-5 my-1 p-0 font-weight-normal" v-if="!showSections['Provided Documents']">
            Provided Documents ({{ NumberOfDocuments }})
          </h3>
          <b-button
            @click="preDownloadDocuments()"
            size="sm"
            variant="primary"
            style="padding: 0 1rem; margin-left:2rem; margin-right:2rem; height: 35px"
          >
            Download Selection
          </b-button>
        </b-row>
        <hr class="mx-3 bg-light" style="height: 5px;" />
      </div>

      <b-card v-if="!isDataReady && isMounted">
        <span class="text-muted ml-4 mb-5"> No provided documents. </span>
      </b-card>

      <b-card bg-variant="light" v-if="!isMounted && !isDataReady">
        <b-overlay :show="true">
          <b-card style="min-height: 100px;" />
          <template v-slot:overlay>
            <div>
              <loading-spinner />
              <p id="loading-label">Loading ...</p>
            </div>
          </template>
        </b-overlay>
      </b-card>

      <b-tabs
        nav-wrapper-class="bg-light text-dark"
        active-nav-item-class="text-uppercase font-weight-bold text-white bg-primary"
        pills
        no-body
        v-if="isDataReady"
        class="mx-3"
      >
        <b-tab
          v-for="(tabMapping, index) in categories"
          :key="index"
          :title="tabMapping"
          v-on:click="switchTab(tabMapping)"
          v-bind:class="[activetab === tabMapping ? 'active mb-3' : 'mb-3']"
        ></b-tab>
      </b-tabs>

      <b-overlay :show="loadingPdf" rounded="sm">
        <b-card
          bg-variant="light"
          v-if="isDataReady"
          style="max-height: 500px; overflow-y: auto;"
          no-body
          class="mx-3 mb-5"
        >
          <b-table
            :items="FilteredDocuments"
            :fields="fields"
            sort-by="appearanceDate"
            :sort-desc.sync="sortDesc"
            :no-sort-reset="true"
            sort-icon-left
            small
            striped
            responsive="sm"
          >
            <template v-for="(field, index) in fields" v-slot:[`head(${field.key})`]="data">
              <b v-bind:key="index" :class="field.headerStyle"> {{ data.label }}</b>
            </template>

            <template v-slot:cell(appearanceDate)="data">
              <span :style="data.field.cellStyle">
                {{ data.value | beautify_date }}
              </span>
            </template>

            <!-- <template v-slot:cell(enterDtm)="data" >                        
                        <span :style="data.field.cellStyle">
                            {{ data.value | beautify_date_time}}
                        </span>
                    </template>                      -->

            <template v-slot:cell(referenceDocumentTypeDsc)="data">
              <b-button
                variant="outline-primary text-info"
                :style="data.field.cellStyle"
                @click="cellClick(data)"
                size="sm"
              >
                {{ data.value }}
              </b-button>
            </template>

            <template v-slot:head(select)>
              <b-form-checkbox class="m-0 checkbox" v-model="allDocumentsChecked" @change="checkAllDocuments" size="sm" />
            </template>

            <template v-slot:cell(select)="data">
              <b-form-checkbox
                size="sm"
                class="m-0 checkbox"
                :disabled="!data.item.isEnabled"
                v-model="data.item.isChecked"
                @change="toggleSelectedDocuments"
              />
            </template>

            <template v-slot:cell(download)="data">
              <div
                class="text-center"
              >
                <b-button
                  :style="data.field.cellStyle"
                  size="sm"
                  :variant="'outline-primary border-white text-' + data.item.listClass"
                  class="mr-2"
                  @click="preDownloadCloudClick(data)"
                >
                  <b-icon-cloud
                    :variant="data.item.listClass"
                  ></b-icon-cloud>
                </b-button>
              </div>
            </template>

            <template v-slot:cell(descriptionText)="data">
              <div :style="data.field.cellStyle" v-b-tooltip.hover :title="data.value.length > 45 ? data.value : ''">
                {{ data.value | truncate(45) }}
              </div>
            </template>

            <template v-slot:cell(partyName)="data">
              <div v-for="(partyName, index) in data.value" v-bind:key="index">
                <span :style="data.field.cellStyle"> {{ partyName }}</span>
              </div>
            </template>

            <template v-slot:cell(nonPartyName)="data">
              <div v-for="(nonParty, index) in data.value" v-bind:key="index">
                <span :style="data.field.cellStyle"> {{ nonParty }}</span>
              </div>
            </template>

            <template v-slot:cell()="data">
              <span class="ml-2" :style="data.field.cellStyle">
                {{ data.value }}
              </span>
            </template>
          </b-table>
        </b-card>
        <template v-slot:overlay>
          <div style="text-align: center">
            <loading-spinner />
            <p id="Downloading-label">Downloading PDF file ...</p>
          </div>
        </template>
      </b-overlay>
    </b-card>

    <b-modal id="progress-modal" :title="progressModalTitle" @shown="startPolling" size="lg">
      <div v-for="(progress, index) in progressValues" :key="index" class="mb-2">
        <span class="progress-label">{{ progress.percentTransfered }}%</span>
        <span class="progress-filename">{{ progress.fileName }}</span>
        <b-progress :value="progress.percentTransfered" variant="success"></b-progress>
      </div>
      <template #modal-footer>
        <b-button @click="cancelDownload">Cancel</b-button>
      </template>
    </b-modal>
  </div>
</template>

<script lang="ts">
import { Component, Vue } from "vue-property-decorator";
import { namespace } from "vuex-class";
import "@store/modules/CivilFileInformation";
import "@store/modules/CommonInformation";
import { civilFileInformationType, referenceDocumentsInfoType } from "@/types/civil";
import { CourtDocumentType, DocumentData } from "@/types/shared";
const civilState = namespace("CivilFileInformation");
const commonState = namespace("CommonInformation");
import CustomOverlay from "../CustomOverlay.vue";
import shared from "../shared";
import { ArchiveInfoType } from "@/types/common";
import {
  CancelPreDownloadInfoType,
} from "@/types/common";
enum fieldTab {
  Categories = 0,
}

@Component({
  components: {
    CustomOverlay,
  },
})
export default class CivilProvidedDocumentsView extends Vue {
  @commonState.State
  public enableArchive!: boolean;

  @civilState.State
  public showSections;

  @civilState.State
  public civilFileInformation!: civilFileInformationType;

  @civilState.State
  public hasNonParty!: boolean;

  @civilState.Action
  public UpdateCivilFile!: (newCivilFileInformation: civilFileInformationType) => void;

  public cancelPreDownloadInfo: CancelPreDownloadInfoType = {
        transferIds: [] // Initialize with an empty array or any default values
  };

  documents: referenceDocumentsInfoType[] = [];
  loadingPdf = false;
  isMounted = false;
  isDataReady = false;
  activetab = "ALL";
  sortDesc = false;
  categories: string[] = [];
  fieldsTab = fieldTab.Categories;
  fields: any = [];
  selectedDocuments = {} as ArchiveInfoType;
  downloadCompleted = true;
  allDocumentsChecked = false;
  progressModalTitle = "Downloading to OneDrive...";
  progressValues: any[] = [];
  pollingInterval: ReturnType<typeof setTimeout> | null = null;

  initialFields = [
    {
      key: "select",
      label: "",
      sortable: false,
      headerStyle: "text-primary",
      cellStyle: "font-size: 16px;",
      tdClass: "border-top",
      thClass: "",
    },
    {
      key: "download",
      label: "Download",
      tdClass: "border-top",
      cellStyle: "font-weight: normal; font-size: 16px; border: 0px",
    },
    { key: "dummy", label: "Dummy", tdClass: "border-top", cellStyle: "font-size:16px" }, // This table never renders the second column
    {
      key: "partyName",
      label: "Party Name",
      sortable: true,
      headerStyle: "text-primary",
      cellStyle: "font-size: 16px;",
    },
    {
      key: "nonPartyName",
      label: "Non Party",
      sortable: true,
      headerStyle: "text-primary",
      cellStyle: "font-size: 16px;",
    },
    {
      key: "referenceDocumentTypeDsc",
      label: "Document Type",
      sortable: false,
      headerStyle: "text-primary",
      cellStyle: "border:0px; font-size: 16px;text-align:left;",
    },
    {
      key: "appearanceDate",
      label: "Appearance Date",
      sortable: true,
      headerStyle: "text",
      cellStyle: "font-size: 16px;",
    },
    // {key:'enterDtm',                 label:'Created Date',    sortable:true,  headerStyle:'text',          cellStyle:'font-size: 16px;'},
    {
      key: "descriptionText",
      label: "Description",
      sortable: false,
      headerStyle: "text",
      cellStyle: "font-size: 12px;",
    },
  ];

  public getDocuments(): void {
    this.documents = this.civilFileInformation.referenceDocumentInfo;
    this.categories = this.civilFileInformation.providedDocumentCategories;
    this.categories.sort();
    if (this.categories.indexOf("ALL") < 0) this.categories.unshift("ALL");
    this.fields = JSON.parse(JSON.stringify(this.initialFields));
    if (!this.hasNonParty) {
      this.fields.splice(2, 1);
    }
    if (this.documents.length > 0) {
      this.isDataReady = true;
    }
    this.isMounted = true;
  }

  mounted() {
    this.getDocuments();
    this.downloadCompleted = true;
    this.selectedDocuments = { zipName: "", csrRequests: [], documentRequests: [], ropRequests: [] };
  }

  public checkAllDocuments(checked) {
    if (this.activetab != "ALL") {
      for (const docInx in this.documents) {
        if (
          this.documents[docInx].referenceDocumentTypeDsc.includes(this.activetab) &&
          this.documents[docInx].isEnabled
        ) {
          this.documents[docInx].isChecked = checked;
        }
      }
    } else {
      for (const docInx in this.documents) {
        if (this.documents[docInx].isEnabled) {
          this.documents[docInx].isChecked = checked;
        }
      }
    }
  }

  public switchTab(tabMapping) {
    this.allDocumentsChecked = false;
    this.activetab = tabMapping;
  }

  public toggleSelectedDocuments() {
    Vue.nextTick(() => {
      const checkedDocs = this.documents.filter((doc) => {
        return doc.isChecked;
      });
      const enabledDocs = this.documents.filter((doc) => {
        return doc.isEnabled;
      });
      if (checkedDocs.length == enabledDocs.length) this.allDocumentsChecked = true;
      else this.allDocumentsChecked = false;
    });
  }

  public cellClick(eventData) {
    this.loadingPdf = true;
    const documentData: DocumentData = {
      appearanceDate: Vue.filter("beautify_date")(eventData.item.appearanceDate),
      courtClass: this.civilFileInformation.detailsData.courtClassCd,
      courtLevel: this.civilFileInformation.detailsData.courtLevelCd,
      documentId: eventData.item.objectGuid,
      documentDescription: eventData.item.referenceDocumentTypeDsc,
      fileId: this.civilFileInformation.fileNumber,
      fileNumberText: this.civilFileInformation.detailsData.fileNumberTxt,
      partyName: eventData.item.partyName,
      location: this.civilFileInformation.detailsData.homeLocationAgencyName
    };
    shared.openDocumentsPdf(CourtDocumentType.ProvidedCivil, documentData);
    this.loadingPdf = false;
  }

  public navigateToLandingPage() {
    this.$router.push({ name: "Home" });
  }

  get FilteredDocuments() {
    return this.documents.filter((doc) => {
      if (this.activetab != "ALL") {
        if (doc.referenceDocumentTypeDsc.includes(this.activetab)) {
          return true;
        }

        return false;
      } else {
        return true;
      }
    });
  }

  get NumberOfDocuments() {
    return this.documents.length;
  }

  public preDownloadDocuments(): void {
    this.documents.forEach(listItem => {
      if (listItem.isChecked) {
        debugger;
        const objGuid = encodeURIComponent(btoa(listItem.objectGuid));
        const filePath = encodeURIComponent(`${this.$route.params.location}/${this.$route.params.fileNumberText}/${this.$route.params.room}`);
        const fileName = encodeURIComponent(`${this.$route.params.fileNumberText}-${listItem.referenceDocumentTypeDsc}-${listItem.appearanceDate.substring(0, 10)}-${listItem.partyName[0]}.pdf`);
        const url = `api/files/upload?objGuid=${objGuid}&filePath=${filePath}&fileName=${fileName}`;

        shared.submitUploadRequest(url, this);
      }
    })
  }

  public cancelDownload() {
    shared.cancelDownload(this);
  }

  // Method to start polling for progress values
  startPolling() {
    shared.startPolling(this);
  }
  
  public stopPolling() {
    shared.stopPolling(this);
  } 

  public cleanUp() {
    if (this.pollingInterval) {
      clearInterval(this.pollingInterval);
      this.pollingInterval = null;
    }
    this.documents.forEach(item => {
      item.isChecked = false;
    });
    this.allDocumentsChecked = false;
  }

  preDownloadCloudClick(data) {
    this.documents[data.index].isChecked = true;
    this.preDownloadDocuments();
  }
}
</script>

<style scoped>
.card {
  border: white;
}
.checkbox {
  padding-top: 4px;
  padding-left: 30px;
}
.progress-filename {
  float: right;
  font-size: smaller;
  vertical-align: middle;
}
</style>
