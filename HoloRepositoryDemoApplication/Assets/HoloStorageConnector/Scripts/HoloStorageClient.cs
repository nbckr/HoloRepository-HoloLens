﻿using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using Microsoft.MixedReality.Toolkit.Utilities;
using Microsoft.MixedReality.Toolkit.Utilities.Gltf.Serialization;
using System.Collections.Generic;
using System.Reflection;
using SimpleJSON;

namespace HoloStorageConnector
{
    /// <summary>
    /// Class <c>HoloStorageClient</c> provided multiple methods to retrieve data from Storage server.
    /// </summary>
    public class HoloStorageClient : MonoBehaviour
    {
        #region Properties
        private static string storageAccessorEndpoint = "http://localhost";
        private static string port = "8080";
        private static string apiVersion = "1.0.0";
        private static string baseUri = $"{storageAccessorEndpoint}:{port}/api/{apiVersion}";        
        private static string webRequestReturnData = null;
        #endregion Properties

        #region Public Method
        /// <summary>
        /// Set base end point Uri
        /// </summary>
        public static void SetEndpoint(string endpoint)
        {
            storageAccessorEndpoint = endpoint;
        }
        public static void SetPort(string portValue)
        {
            port = portValue;
        }
        public static void SetApiVersion(string version)
        {
            apiVersion = version;
        }

        /// <summary>
        /// Method <c>GetMultiplePatients</c> is used to retrieve multiple patients meta data from Storage server 
        /// Result will be store in the parameter list 
        /// </summary>
        /// <param name="patientList">Patient object list, used to store information</param>
        /// <param name="IDs">IDs of querying patients</param>
        /// <returns></returns>
        public static IEnumerator GetMultiplePatients(List<Patient> patientList, string IDs)
        {
            string multiplePatientUri = $"{baseUri}/patients?pid={IDs}";
            yield return GetRequest(multiplePatientUri);

            patientList.Clear();
            string[] ids = IDs.Split(',');
            if (webRequestReturnData != null)
            {
                JSONNode initialJsonData = JSON.Parse(webRequestReturnData);
                foreach (string id in ids)
                {
                    JSONNode data = initialJsonData[id];
                    Patient patient = JsonToPatient(data, id);
                    if (patient.pid != null)
                    {
                        patientList.Add(patient);
                    }
                }
            }
        }

        /// <summary>
        /// Method <c>GetPatient</c> allows user retrieve single patient meta data from Storage server by patient ID. 
        /// </summary>
        /// <param name="resultPatient">Patient object, used to store information</param>
        /// <param name="patientID">ID of querying patient</param>
        /// <returns></returns>
        public static IEnumerator GetPatient(Patient resultPatient, string patientID)
        {
            string getPatientUri = $"{baseUri}/patients/{patientID}";
            yield return GetRequest(getPatientUri);

            if (webRequestReturnData != null)
            {
                JSONNode patientJson = JSON.Parse(webRequestReturnData);
                Patient receivedPatient = JsonToPatient(patientJson, patientID);
                CopyProperties(receivedPatient, resultPatient);
            }                
        }

        /// <summary>
        /// Method <c>GetMultipleHolograms</c> is used to retrieve multiple hologram meta data from Storage server
        /// </summary>
        /// <param name="hologramList">Hologram object list, used to store information</param>
        /// <param name="IDs">IDs of querying holograms</param>
        /// <returns></returns>
        public static IEnumerator GetMultipleHolograms(List<Hologram> hologramList, string IDs)
        {
            string multipleHologramUri = $"{baseUri}/holograms?hid={IDs}";        
            yield return GetRequest(multipleHologramUri);

            hologramList.Clear();
            string[] ids = IDs.Split(',');
            if (webRequestReturnData != null)
            {
                JSONNode initialJsonData = JSON.Parse(webRequestReturnData);
                foreach (string id in ids)
                {
                    JSONNode data = initialJsonData[id];
                    JSONArray JsonArray = data.AsArray;

                    if(JsonArray.Count == 0)
                    {
                        Debug.LogError($"Response from server is empty with this ID: {id}");
                    }

                    foreach (JSONNode hologramJson in JsonArray)
                    {
                        Hologram hologram = JsonToHologram(hologramJson, id);
                        hologramList.Add(hologram);
                    }
                }
            }
        }

        /// <summary>
        /// Method <c>GetHologram</c> allows user retrieve single hologram from Storage server by hologram ID
        /// </summary>
        /// <param name="resultHologram">Hologram object, used to store information</param>
        /// <param name="holgramID">ID of querying hologram</param>
        /// <returns></returns>
        public static IEnumerator GetHologram(Hologram resultHologram, string holgramID)
        {
            string getHologramUri = $"{baseUri}/holograms/{holgramID}";
            yield return GetRequest(getHologramUri);

            if (webRequestReturnData != null)
            {
                JSONNode hologramJson = JSON.Parse(webRequestReturnData);
                Hologram receivedHologram = JsonToHologram(hologramJson, holgramID);
                CopyProperties(receivedHologram, resultHologram);
            }             
        }

        /// <summary>
        /// Method <c>GetMultipleAuthors</c> is used to retrieve multiple authors meta data from Storage server
        /// </summary>
        /// <param name="authorList">Author object list, used to store information</param>
        /// <param name="IDs">IDs of querying authors</param>
        /// <returns></returns>
        public static IEnumerator GetMultipleAuthors(List<Author> authorList, string IDs)
        {
            string multipleAuthorUri = $"{baseUri}/authors?aid={IDs}";        
            yield return GetRequest(multipleAuthorUri);

            authorList.Clear();
            string[] ids = IDs.Split(',');
            if (webRequestReturnData != null)
            {
                JSONNode initialJsonData = JSON.Parse(webRequestReturnData);
                foreach (string id in ids)
                {
                    JSONNode data = initialJsonData[id];
                    Author author = JsonToAuthor(data, id);
                    if (author.aid != null)
                    {
                        authorList.Add(author);
                    }
                }
            }
        }

        /// <summary>
        /// Method <c>GetAuthor</c> allows user retrieve single author from Storage server by hologram ID
        /// </summary>
        /// <param name="resultAuthor">Author object, used to store information</param>
        /// <param name="authorID">ID of querying author</param>
        /// <returns></returns>
        public static IEnumerator GetAuthor(Author resultAuthor, string authorID)
        {
            string getAuthorUri = $"{baseUri}/authors/{authorID}";
            yield return GetRequest(getAuthorUri);

            if (webRequestReturnData != null)
            {
                JSONNode authorJson = JSON.Parse(webRequestReturnData);
                Author receivedAuthor = JsonToAuthor(authorJson, authorID);
                CopyProperties(receivedAuthor, resultAuthor);
            }
        }

        /// <summary>
        /// Method <c>LoadHologram</c> is used to load hologram from Storage server
        /// It requires thehologram ID as the parameter 
        /// </summary>
        /// <param name="hologramID">ID of Hologram</param>
        public static async void LoadHologram(string hologramID, HologramInstantiationSettings setting = null)
        {
            if (setting == null)
            {
                setting = new HologramInstantiationSettings();
            }

            //string getHologramUri = $"{BaseUri}{apiPrefix}/holograms/{hologramID}/download";
            string getHologramUri = "https://holoblob.blob.core.windows.net/test/DamagedHelmet-18486331-5441-4271-8169-fcac6b7d8c29.glb";      

            Response response = new Response();
            try
            {
                response = await Rest.GetAsync(getHologramUri);
            }
            catch (Exception e)
            {
                Debug.LogError(e.Message);
            }

            if (!response.Successful)
            {
                Debug.LogError($"Failed to get glb model from {getHologramUri}");
                return;
            }

            var gltfObject = GltfUtility.GetGltfObjectFromGlb(response.ResponseData);

            try
            {
                GameObject loadedObject = await gltfObject.ConstructAsync();
                HologramInstantiationSettings.Initialize(loadedObject, setting);
            }
            catch (Exception e)
            {
                Debug.LogError($"{e.Message}\n{e.StackTrace}");
                return;
            }
        }
        #endregion Public Method

        #region Common Method
        /// <summary>
        /// Common method <c>GetRequest</c> is used to handle web request 
        /// </summary>
        /// <param name="uri">Endpoint for the web request</param>
        /// <returns></returns>
        private static IEnumerator GetRequest(string uri)
        {
            using (UnityWebRequest webRequest = UnityWebRequest.Get(uri))
            {
                yield return webRequest.SendWebRequest();

                webRequestReturnData = null;
                if (webRequest.isNetworkError)
                {
                    Debug.LogError("Web request Error! [Error message]: " + webRequest.error);
                }
                else
                {
                    webRequestReturnData = webRequest.downloadHandler.text;
                }
            }
        }

        /// <summary>
        /// Common method <c>CopyProperties</c> is used to map the preoperties between two objects 
        /// </summary>
        /// <param name="source">Source object</param>
        /// <param name="destination">Target object</param>
        private static void CopyProperties(object source, object destination)
        {
            PropertyInfo[] destinationProperties = destination.GetType().GetProperties();
            foreach (PropertyInfo destinationPi in destinationProperties)
            {
                PropertyInfo sourcePi = source.GetType().GetProperty(destinationPi.Name);
                destinationPi.SetValue(destination, sourcePi.GetValue(source, null), null);
            }
        }

        /// <summary>
        /// Method <c>JsonToPatient</c> map the json data into Patient object 
        /// </summary>
        /// <param name="json">Initial json data</param>
        /// <returns>Patient object with retrieved information</returns>
        public static Patient JsonToPatient(JSONNode json, string id)
        {
            Patient patient = new Patient();

            if (json["pid"].Value == "")
            {
                Debug.LogError($"Response from server is empty with this patient ID: {id}");
                return patient;
            }

            try
            {
                patient.pid = json["pid"].Value;
                patient.gender = json["gender"].Value;
                patient.birthDate = json["birthDate"].Value;

                PersonName name = new PersonName();
                name.title = json["name"]["title"].Value;
                name.full = json["name"]["full"].Value;
                name.given = json["name"]["given"].Value;
                name.family = json["name"]["family"].Value;
                patient.name = name;
            }
            catch (Exception e)
            {
                Debug.LogError("Failed to map patient from response data! \n[Error message]: " + e);
            }
                    
            return patient;
        }

        /// <summary>
        /// Method <c>JsonToHologram</c> map the json data into Hologram object
        /// </summary>
        /// <param name="json">Initial json data</param>
        /// <returns>Hologram object with retrieved information</returns>
        public static Hologram JsonToHologram(JSONNode json, string id)
        {
            Hologram hologram = new Hologram();

            if (json["hid"].Value == "")
            {
                Debug.LogError($"Response from server is empty with this ID: {id}");
                return hologram;
            }

            try
            {
                hologram.hid = json["hid"].Value;
                hologram.title = json["title"].Value;
                hologram.description = json["description"].Value;
                hologram.contentType = json["contentType"].Value;
                hologram.fileSizeInkb = json["fileSizeInkb"].AsInt;
                hologram.bodySite = json["bodySite"].Value;
                hologram.dateOfImaging = json["dateOfImaging"].Value;
                hologram.creationDate = json["creationDate"].Value;
                hologram.creationMode = json["creationMode"].Value;
                hologram.creationDescription = json["creationDescription"].Value;
                hologram.aid = json["aid"].Value;
                hologram.pid = json["pid"].Value;
            }
            catch (Exception e)
            {
                Debug.LogError("Failed to map hologram from response data! \n[Error message]: " + e);
            }            
            return hologram;
        }

        /// <summary>
        /// Method <c>JsonToAuthor</c> map the json data into Author object
        /// </summary>
        /// <param name="json">Initial json data</param>
        /// <returns>Author object with retrieved information</returns>
        public static Author JsonToAuthor(JSONNode json, string id)
        {
            Author author = new Author();

            if (json["aid"].Value == "")
            {
                Debug.LogError($"Response from server is empty with this author ID: {id}");
                return author;
            }

            try
            {
                author.aid = json["aid"].Value;

                PersonName name = new PersonName();
                name.full = json["name"]["full"].Value;
                name.title = json["name"]["title"].Value;               
                name.given = json["name"]["given"].Value;
                name.family = json["name"]["family"].Value;
                author.name = name;
            }
            catch (Exception e)
            {
                Debug.LogError("Failed to map author from response data! \n[Error message]: " + e);
            }
            return author;
        }
        #endregion Commom Method
    }
}