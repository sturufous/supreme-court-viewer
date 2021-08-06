// import Axios from "axios";

export const SessionManager = {
    // TODO: once there's an api parameter, we need to use it to set enableArchive
    getSettings: async function(store) {
        try {
            //var response = await Axios.get('');            
            const enableArchive = false //response.data.enableArchive;
            store.commit("CommonInformation/setEnableArchive",enableArchive);
            
            return true;
        }
        catch (error) {
            console.log(error);  
            return false;
        }
    }
}
