/**
* Copyright 2018, 2019 IBM Corp. All Rights Reserved.
*
* Licensed under the Apache License, Version 2.0 (the "License");
* you may not use this file except in compliance with the License.
* You may obtain a copy of the License at
*
*      http://www.apache.org/licenses/LICENSE-2.0
*
* Unless required by applicable law or agreed to in writing, software
* distributed under the License is distributed on an "AS IS" BASIS,
* WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
* See the License for the specific language governing permissions and
* limitations under the License.
*
*/

using System.Collections.Generic;
using System.Text;
using IBM.Cloud.SDK;
using IBM.Cloud.SDK.Connection;
using IBM.Cloud.SDK.Utilities;
using IBM.Watson.CompareComply.V1.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using UnityEngine.Networking;

namespace IBM.Watson.CompareComply.V1
{
    public class CompareComplyService : BaseService
    {
        private const string serviceId = "compare-comply";
        private const string defaultUrl = "https://gateway.watsonplatform.net/compare-comply/api";

        #region Credentials
        /// <summary>
        /// Gets and sets the credentials of the service. Replace the default endpoint if endpoint is defined.
        /// </summary>
        public Credentials Credentials
        {
            get { return credentials; }
            set
            {
                credentials = value;
                if (!string.IsNullOrEmpty(credentials.Url))
                {
                    Url = credentials.Url;
                }
            }
        }
        #endregion

        #region Url
        /// <summary>
        /// Gets and sets the endpoint URL for the service.
        /// </summary>
        public string Url
        {
            get { return url; }
            set { url = value; }
        }
        #endregion

        #region VersionDate
        private string versionDate;
        /// <summary>
        /// Gets and sets the versionDate of the service.
        /// </summary>
        public string VersionDate
        {
            get { return versionDate; }
            set { versionDate = value; }
        }
        #endregion

        #region DisableSslVerification
        private bool disableSslVerification = false;
        /// <summary>
        /// Gets and sets the option to disable ssl verification
        /// </summary>
        public bool DisableSslVerification
        {
            get { return disableSslVerification; }
            set { disableSslVerification = value; }
        }
        #endregion

        /// <summary>
        /// CompareComplyService constructor.
        /// </summary>
        /// <param name="versionDate">The service version date in `yyyy-mm-dd` format.</param>
        public CompareComplyService(string versionDate) : base(versionDate, serviceId)
        {
            VersionDate = versionDate;
        }

        /// <summary>
        /// CompareComplyService constructor.
        /// </summary>
        /// <param name="versionDate">The service version date in `yyyy-mm-dd` format.</param>
        /// <param name="credentials">The service credentials.</param>
        public CompareComplyService(string versionDate, Credentials credentials) : base(versionDate, credentials, serviceId)
        {
            if (string.IsNullOrEmpty(versionDate))
            {
                throw new ArgumentNullException("A versionDate (format `yyyy-mm-dd`) is required to create an instance of CompareComplyService");
            }
            else
            {
                VersionDate = versionDate;
            }

            if (credentials.HasCredentials() || credentials.HasIamTokenData())
            {
                Credentials = credentials;

                if (string.IsNullOrEmpty(credentials.Url))
                {
                    credentials.Url = defaultUrl;
                }
            }
            else
            {
                throw new IBMException("Please provide a username and password or authorization token to use the CompareComply service. For more information, see https://github.com/watson-developer-cloud/unity-sdk/#configuring-your-service-credentials");
            }
        }

        /// <summary>
        /// Convert file to HTML.
        ///
        /// Convert an uploaded file to HTML.
        /// </summary>
        /// <param name="callback">The callback function that is invoked when the operation completes.</param>
        /// <param name="file">The file to convert.</param>
        /// <param name="modelId">The analysis model to be used by the service. For the `/v1/element_classification` and
        /// `/v1/comparison` methods, the default is `contracts`. For the `/v1/tables` method, the default is `tables`.
        /// These defaults apply to the standalone methods as well as to the methods' use in batch-processing requests.
        /// (optional)</param>
        /// <param name="fileContentType">The content type of file. (optional)</param>
        /// <param name="customData">A Dictionary<string, object> of data that will be passed to the callback. The raw
        /// json output from the REST call will be passed in this object as the value of the 'json'
        /// key.</string></param>
        /// <returns><see cref="HTMLReturn" />HTMLReturn</returns>
        public bool ConvertToHtml(Callback<HTMLReturn> callback, System.IO.FileStream file, Dictionary<string, object> customData = null, string modelId = null, string fileContentType = null)
        {
            if (callback == null)
                throw new ArgumentNullException("`callback` is required for `ConvertToHtml`");
            if (file == null)
                throw new ArgumentNullException("`file` is required for `ConvertToHtml`");

            RequestObject<HTMLReturn> req = new RequestObject<HTMLReturn>
            {
                Callback = callback,
                HttpMethod = UnityWebRequest.kHttpVerbPOST,
                DisableSslVerification = DisableSslVerification,
                CustomData = customData == null ? new Dictionary<string, object>() : customData
            };

            if (req.CustomData.ContainsKey(Constants.String.CUSTOM_REQUEST_HEADERS))
            {
                foreach (KeyValuePair<string, string> kvp in req.CustomData[Constants.String.CUSTOM_REQUEST_HEADERS] as Dictionary<string, string>)
                {
                    req.Headers.Add(kvp.Key, kvp.Value);
                }
            }

            foreach(KeyValuePair<string, string> kvp in Common.GetDefaultheaders("compare-comply", "V1", "ConvertToHtml"))
            {
                req.Headers.Add(kvp.Key, kvp.Value);
            }

            req.Parameters["version"] = VersionDate;
            req.Forms = new Dictionary<string, RESTConnector.Form>();
            req.Forms["file"] = new RESTConnector.Form(file, file.Name, fileContentType);
            if (!string.IsNullOrEmpty(modelId))
            {
                req.Parameters["model_id"] = modelId;
            }

            req.OnResponse = OnConvertToHtmlResponse;

            RESTConnector connector = RESTConnector.GetConnector(Credentials, "/v1/html_conversion");
            if (connector == null)
            {
                return false;
            }

            return connector.Send(req);
        }

        private void OnConvertToHtmlResponse(RESTConnector.Request req, RESTConnector.Response resp)
        {
            DetailedResponse<HTMLReturn> response = new DetailedResponse<HTMLReturn>();
            Dictionary<string, object> customData = ((RequestObject<HTMLReturn>)req).CustomData;
            foreach (KeyValuePair<string, string> kvp in resp.Headers)
            {
                response.Headers.Add(kvp.Key, kvp.Value);
            }
            response.StatusCode = resp.HttpResponseCode;

            try
            {
                string json = Encoding.UTF8.GetString(resp.Data);
                response.Result = JsonConvert.DeserializeObject<HTMLReturn>(json);
                customData.Add("json", json);
            }
            catch (Exception e)
            {
                Log.Error("CompareComplyService.OnConvertToHtmlResponse()", "Exception: {0}", e.ToString());
                resp.Success = false;
            }

            if (((RequestObject<HTMLReturn>)req).Callback != null)
                ((RequestObject<HTMLReturn>)req).Callback(response, resp.Error, customData);
        }
        /// <summary>
        /// Classify the elements of a document.
        ///
        /// Analyze an uploaded file's structural and semantic elements.
        /// </summary>
        /// <param name="callback">The callback function that is invoked when the operation completes.</param>
        /// <param name="file">The file to classify.</param>
        /// <param name="modelId">The analysis model to be used by the service. For the `/v1/element_classification` and
        /// `/v1/comparison` methods, the default is `contracts`. For the `/v1/tables` method, the default is `tables`.
        /// These defaults apply to the standalone methods as well as to the methods' use in batch-processing requests.
        /// (optional)</param>
        /// <param name="fileContentType">The content type of file. (optional)</param>
        /// <param name="customData">A Dictionary<string, object> of data that will be passed to the callback. The raw
        /// json output from the REST call will be passed in this object as the value of the 'json'
        /// key.</string></param>
        /// <returns><see cref="ClassifyReturn" />ClassifyReturn</returns>
        public bool ClassifyElements(Callback<ClassifyReturn> callback, System.IO.FileStream file, Dictionary<string, object> customData = null, string modelId = null, string fileContentType = null)
        {
            if (callback == null)
                throw new ArgumentNullException("`callback` is required for `ClassifyElements`");
            if (file == null)
                throw new ArgumentNullException("`file` is required for `ClassifyElements`");

            RequestObject<ClassifyReturn> req = new RequestObject<ClassifyReturn>
            {
                Callback = callback,
                HttpMethod = UnityWebRequest.kHttpVerbPOST,
                DisableSslVerification = DisableSslVerification,
                CustomData = customData == null ? new Dictionary<string, object>() : customData
            };

            if (req.CustomData.ContainsKey(Constants.String.CUSTOM_REQUEST_HEADERS))
            {
                foreach (KeyValuePair<string, string> kvp in req.CustomData[Constants.String.CUSTOM_REQUEST_HEADERS] as Dictionary<string, string>)
                {
                    req.Headers.Add(kvp.Key, kvp.Value);
                }
            }

            foreach(KeyValuePair<string, string> kvp in Common.GetDefaultheaders("compare-comply", "V1", "ClassifyElements"))
            {
                req.Headers.Add(kvp.Key, kvp.Value);
            }

            req.Parameters["version"] = VersionDate;
            req.Forms = new Dictionary<string, RESTConnector.Form>();
            req.Forms["file"] = new RESTConnector.Form(file, file.Name, fileContentType);
            if (!string.IsNullOrEmpty(modelId))
            {
                req.Parameters["model_id"] = modelId;
            }

            req.OnResponse = OnClassifyElementsResponse;

            RESTConnector connector = RESTConnector.GetConnector(Credentials, "/v1/element_classification");
            if (connector == null)
            {
                return false;
            }

            return connector.Send(req);
        }

        private void OnClassifyElementsResponse(RESTConnector.Request req, RESTConnector.Response resp)
        {
            DetailedResponse<ClassifyReturn> response = new DetailedResponse<ClassifyReturn>();
            Dictionary<string, object> customData = ((RequestObject<ClassifyReturn>)req).CustomData;
            foreach (KeyValuePair<string, string> kvp in resp.Headers)
            {
                response.Headers.Add(kvp.Key, kvp.Value);
            }
            response.StatusCode = resp.HttpResponseCode;

            try
            {
                string json = Encoding.UTF8.GetString(resp.Data);
                response.Result = JsonConvert.DeserializeObject<ClassifyReturn>(json);
                customData.Add("json", json);
            }
            catch (Exception e)
            {
                Log.Error("CompareComplyService.OnClassifyElementsResponse()", "Exception: {0}", e.ToString());
                resp.Success = false;
            }

            if (((RequestObject<ClassifyReturn>)req).Callback != null)
                ((RequestObject<ClassifyReturn>)req).Callback(response, resp.Error, customData);
        }
        /// <summary>
        /// Extract a document's tables.
        ///
        /// Extract and analyze an uploaded file's tables.
        /// </summary>
        /// <param name="callback">The callback function that is invoked when the operation completes.</param>
        /// <param name="file">The file on which to run table extraction.</param>
        /// <param name="modelId">The analysis model to be used by the service. For the `/v1/element_classification` and
        /// `/v1/comparison` methods, the default is `contracts`. For the `/v1/tables` method, the default is `tables`.
        /// These defaults apply to the standalone methods as well as to the methods' use in batch-processing requests.
        /// (optional)</param>
        /// <param name="fileContentType">The content type of file. (optional)</param>
        /// <param name="customData">A Dictionary<string, object> of data that will be passed to the callback. The raw
        /// json output from the REST call will be passed in this object as the value of the 'json'
        /// key.</string></param>
        /// <returns><see cref="TableReturn" />TableReturn</returns>
        public bool ExtractTables(Callback<TableReturn> callback, System.IO.FileStream file, Dictionary<string, object> customData = null, string modelId = null, string fileContentType = null)
        {
            if (callback == null)
                throw new ArgumentNullException("`callback` is required for `ExtractTables`");
            if (file == null)
                throw new ArgumentNullException("`file` is required for `ExtractTables`");

            RequestObject<TableReturn> req = new RequestObject<TableReturn>
            {
                Callback = callback,
                HttpMethod = UnityWebRequest.kHttpVerbPOST,
                DisableSslVerification = DisableSslVerification,
                CustomData = customData == null ? new Dictionary<string, object>() : customData
            };

            if (req.CustomData.ContainsKey(Constants.String.CUSTOM_REQUEST_HEADERS))
            {
                foreach (KeyValuePair<string, string> kvp in req.CustomData[Constants.String.CUSTOM_REQUEST_HEADERS] as Dictionary<string, string>)
                {
                    req.Headers.Add(kvp.Key, kvp.Value);
                }
            }

            foreach(KeyValuePair<string, string> kvp in Common.GetDefaultheaders("compare-comply", "V1", "ExtractTables"))
            {
                req.Headers.Add(kvp.Key, kvp.Value);
            }

            req.Parameters["version"] = VersionDate;
            req.Forms = new Dictionary<string, RESTConnector.Form>();
            req.Forms["file"] = new RESTConnector.Form(file, file.Name, fileContentType);
            if (!string.IsNullOrEmpty(modelId))
            {
                req.Parameters["model_id"] = modelId;
            }

            req.OnResponse = OnExtractTablesResponse;

            RESTConnector connector = RESTConnector.GetConnector(Credentials, "/v1/tables");
            if (connector == null)
            {
                return false;
            }

            return connector.Send(req);
        }

        private void OnExtractTablesResponse(RESTConnector.Request req, RESTConnector.Response resp)
        {
            DetailedResponse<TableReturn> response = new DetailedResponse<TableReturn>();
            Dictionary<string, object> customData = ((RequestObject<TableReturn>)req).CustomData;
            foreach (KeyValuePair<string, string> kvp in resp.Headers)
            {
                response.Headers.Add(kvp.Key, kvp.Value);
            }
            response.StatusCode = resp.HttpResponseCode;

            try
            {
                string json = Encoding.UTF8.GetString(resp.Data);
                response.Result = JsonConvert.DeserializeObject<TableReturn>(json);
                customData.Add("json", json);
            }
            catch (Exception e)
            {
                Log.Error("CompareComplyService.OnExtractTablesResponse()", "Exception: {0}", e.ToString());
                resp.Success = false;
            }

            if (((RequestObject<TableReturn>)req).Callback != null)
                ((RequestObject<TableReturn>)req).Callback(response, resp.Error, customData);
        }
        /// <summary>
        /// Compare two documents.
        ///
        /// Compare two uploaded input files. Uploaded files must be in the same file format.
        /// </summary>
        /// <param name="callback">The callback function that is invoked when the operation completes.</param>
        /// <param name="file1">The first file to compare.</param>
        /// <param name="file2">The second file to compare.</param>
        /// <param name="file1Label">A text label for the first file. (optional, default to file_1)</param>
        /// <param name="file2Label">A text label for the second file. (optional, default to file_2)</param>
        /// <param name="modelId">The analysis model to be used by the service. For the `/v1/element_classification` and
        /// `/v1/comparison` methods, the default is `contracts`. For the `/v1/tables` method, the default is `tables`.
        /// These defaults apply to the standalone methods as well as to the methods' use in batch-processing requests.
        /// (optional)</param>
        /// <param name="file1ContentType">The content type of file1. (optional)</param>
        /// <param name="file2ContentType">The content type of file2. (optional)</param>
        /// <param name="customData">A Dictionary<string, object> of data that will be passed to the callback. The raw
        /// json output from the REST call will be passed in this object as the value of the 'json'
        /// key.</string></param>
        /// <returns><see cref="CompareReturn" />CompareReturn</returns>
        public bool CompareDocuments(Callback<CompareReturn> callback, System.IO.FileStream file1, System.IO.FileStream file2, Dictionary<string, object> customData = null, string file1Label = null, string file2Label = null, string modelId = null, string file1ContentType = null, string file2ContentType = null)
        {
            if (callback == null)
                throw new ArgumentNullException("`callback` is required for `CompareDocuments`");
            if (file1 == null)
                throw new ArgumentNullException("`file1` is required for `CompareDocuments`");
            if (file2 == null)
                throw new ArgumentNullException("`file2` is required for `CompareDocuments`");

            RequestObject<CompareReturn> req = new RequestObject<CompareReturn>
            {
                Callback = callback,
                HttpMethod = UnityWebRequest.kHttpVerbPOST,
                DisableSslVerification = DisableSslVerification,
                CustomData = customData == null ? new Dictionary<string, object>() : customData
            };

            if (req.CustomData.ContainsKey(Constants.String.CUSTOM_REQUEST_HEADERS))
            {
                foreach (KeyValuePair<string, string> kvp in req.CustomData[Constants.String.CUSTOM_REQUEST_HEADERS] as Dictionary<string, string>)
                {
                    req.Headers.Add(kvp.Key, kvp.Value);
                }
            }

            foreach(KeyValuePair<string, string> kvp in Common.GetDefaultheaders("compare-comply", "V1", "CompareDocuments"))
            {
                req.Headers.Add(kvp.Key, kvp.Value);
            }

            req.Parameters["version"] = VersionDate;
            req.Forms = new Dictionary<string, RESTConnector.Form>();
            req.Forms["file_1"] = new RESTConnector.Form(file1, file1.Name, file1ContentType);
            req.Forms["file_2"] = new RESTConnector.Form(file2, file2.Name, file2ContentType);
            if (!string.IsNullOrEmpty(file1Label))
            {
                req.Parameters["file_1_label"] = file1Label;
            }
            if (!string.IsNullOrEmpty(file2Label))
            {
                req.Parameters["file_2_label"] = file2Label;
            }
            if (!string.IsNullOrEmpty(modelId))
            {
                req.Parameters["model_id"] = modelId;
            }

            req.OnResponse = OnCompareDocumentsResponse;

            RESTConnector connector = RESTConnector.GetConnector(Credentials, "/v1/comparison");
            if (connector == null)
            {
                return false;
            }

            return connector.Send(req);
        }

        private void OnCompareDocumentsResponse(RESTConnector.Request req, RESTConnector.Response resp)
        {
            DetailedResponse<CompareReturn> response = new DetailedResponse<CompareReturn>();
            Dictionary<string, object> customData = ((RequestObject<CompareReturn>)req).CustomData;
            foreach (KeyValuePair<string, string> kvp in resp.Headers)
            {
                response.Headers.Add(kvp.Key, kvp.Value);
            }
            response.StatusCode = resp.HttpResponseCode;

            try
            {
                string json = Encoding.UTF8.GetString(resp.Data);
                response.Result = JsonConvert.DeserializeObject<CompareReturn>(json);
                customData.Add("json", json);
            }
            catch (Exception e)
            {
                Log.Error("CompareComplyService.OnCompareDocumentsResponse()", "Exception: {0}", e.ToString());
                resp.Success = false;
            }

            if (((RequestObject<CompareReturn>)req).Callback != null)
                ((RequestObject<CompareReturn>)req).Callback(response, resp.Error, customData);
        }
        /// <summary>
        /// Add feedback.
        ///
        /// Adds feedback in the form of _labels_ from a subject-matter expert (SME) to a governing document.
        /// **Important:** Feedback is not immediately incorporated into the training model, nor is it guaranteed to be
        /// incorporated at a later date. Instead, submitted feedback is used to suggest future updates to the training
        /// model.
        /// </summary>
        /// <param name="callback">The callback function that is invoked when the operation completes.</param>
        /// <param name="feedbackData">Feedback data for submission.</param>
        /// <param name="userId">An optional string identifying the user. (optional)</param>
        /// <param name="comment">An optional comment on or description of the feedback. (optional)</param>
        /// <param name="customData">A Dictionary<string, object> of data that will be passed to the callback. The raw
        /// json output from the REST call will be passed in this object as the value of the 'json'
        /// key.</string></param>
        /// <returns><see cref="FeedbackReturn" />FeedbackReturn</returns>
        public bool AddFeedback(Callback<FeedbackReturn> callback, FeedbackDataInput feedbackData, Dictionary<string, object> customData = null, string userId = null, string comment = null)
        {
            if (callback == null)
                throw new ArgumentNullException("`callback` is required for `AddFeedback`");
            if (feedbackData == null)
                throw new ArgumentNullException("`feedbackData` is required for `AddFeedback`");

            RequestObject<FeedbackReturn> req = new RequestObject<FeedbackReturn>
            {
                Callback = callback,
                HttpMethod = UnityWebRequest.kHttpVerbPOST,
                DisableSslVerification = DisableSslVerification,
                CustomData = customData == null ? new Dictionary<string, object>() : customData
            };

            if (req.CustomData.ContainsKey(Constants.String.CUSTOM_REQUEST_HEADERS))
            {
                foreach (KeyValuePair<string, string> kvp in req.CustomData[Constants.String.CUSTOM_REQUEST_HEADERS] as Dictionary<string, string>)
                {
                    req.Headers.Add(kvp.Key, kvp.Value);
                }
            }

            foreach(KeyValuePair<string, string> kvp in Common.GetDefaultheaders("compare-comply", "V1", "AddFeedback"))
            {
                req.Headers.Add(kvp.Key, kvp.Value);
            }

            req.Parameters["version"] = VersionDate;
            req.Headers["Content-Type"] = "application/json";
            req.Headers["Accept"] = "application/json";

            JObject bodyObject = new JObject();
            if (feedbackData != null)
                bodyObject["feedback_data"] = JToken.FromObject(feedbackData);
            if (!string.IsNullOrEmpty(userId))
                bodyObject["user_id"] = userId;
            if (!string.IsNullOrEmpty(comment))
                bodyObject["comment"] = comment;
            req.Send = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(bodyObject));

            req.OnResponse = OnAddFeedbackResponse;

            RESTConnector connector = RESTConnector.GetConnector(Credentials, "/v1/feedback");
            if (connector == null)
            {
                return false;
            }

            return connector.Send(req);
        }

        private void OnAddFeedbackResponse(RESTConnector.Request req, RESTConnector.Response resp)
        {
            DetailedResponse<FeedbackReturn> response = new DetailedResponse<FeedbackReturn>();
            Dictionary<string, object> customData = ((RequestObject<FeedbackReturn>)req).CustomData;
            foreach (KeyValuePair<string, string> kvp in resp.Headers)
            {
                response.Headers.Add(kvp.Key, kvp.Value);
            }
            response.StatusCode = resp.HttpResponseCode;

            try
            {
                string json = Encoding.UTF8.GetString(resp.Data);
                response.Result = JsonConvert.DeserializeObject<FeedbackReturn>(json);
                customData.Add("json", json);
            }
            catch (Exception e)
            {
                Log.Error("CompareComplyService.OnAddFeedbackResponse()", "Exception: {0}", e.ToString());
                resp.Success = false;
            }

            if (((RequestObject<FeedbackReturn>)req).Callback != null)
                ((RequestObject<FeedbackReturn>)req).Callback(response, resp.Error, customData);
        }
        /// <summary>
        /// Deletes a specified feedback entry.
        /// </summary>
        /// <param name="callback">The callback function that is invoked when the operation completes.</param>
        /// <param name="feedbackId">A string that specifies the feedback entry to be deleted from the document.</param>
        /// <param name="modelId">The analysis model to be used by the service. For the `/v1/element_classification` and
        /// `/v1/comparison` methods, the default is `contracts`. For the `/v1/tables` method, the default is `tables`.
        /// These defaults apply to the standalone methods as well as to the methods' use in batch-processing requests.
        /// (optional)</param>
        /// <param name="customData">A Dictionary<string, object> of data that will be passed to the callback. The raw
        /// json output from the REST call will be passed in this object as the value of the 'json'
        /// key.</string></param>
        /// <returns><see cref="FeedbackDeleted" />FeedbackDeleted</returns>
        public bool DeleteFeedback(Callback<FeedbackDeleted> callback, string feedbackId, Dictionary<string, object> customData = null, string modelId = null)
        {
            if (callback == null)
                throw new ArgumentNullException("`callback` is required for `DeleteFeedback`");
            if (string.IsNullOrEmpty(feedbackId))
                throw new ArgumentNullException("`feedbackId` is required for `DeleteFeedback`");

            RequestObject<FeedbackDeleted> req = new RequestObject<FeedbackDeleted>
            {
                Callback = callback,
                HttpMethod = UnityWebRequest.kHttpVerbDELETE,
                DisableSslVerification = DisableSslVerification,
                CustomData = customData == null ? new Dictionary<string, object>() : customData
            };

            if (req.CustomData.ContainsKey(Constants.String.CUSTOM_REQUEST_HEADERS))
            {
                foreach (KeyValuePair<string, string> kvp in req.CustomData[Constants.String.CUSTOM_REQUEST_HEADERS] as Dictionary<string, string>)
                {
                    req.Headers.Add(kvp.Key, kvp.Value);
                }
            }

            foreach(KeyValuePair<string, string> kvp in Common.GetDefaultheaders("compare-comply", "V1", "DeleteFeedback"))
            {
                req.Headers.Add(kvp.Key, kvp.Value);
            }

            req.Parameters["version"] = VersionDate;
            if (!string.IsNullOrEmpty(modelId))
            {
                req.Parameters["model_id"] = modelId;
            }

            req.OnResponse = OnDeleteFeedbackResponse;

            RESTConnector connector = RESTConnector.GetConnector(Credentials, string.Format("/v1/feedback/{0}", feedbackId));
            if (connector == null)
            {
                return false;
            }

            return connector.Send(req);
        }

        private void OnDeleteFeedbackResponse(RESTConnector.Request req, RESTConnector.Response resp)
        {
            DetailedResponse<FeedbackDeleted> response = new DetailedResponse<FeedbackDeleted>();
            Dictionary<string, object> customData = ((RequestObject<FeedbackDeleted>)req).CustomData;
            foreach (KeyValuePair<string, string> kvp in resp.Headers)
            {
                response.Headers.Add(kvp.Key, kvp.Value);
            }
            response.StatusCode = resp.HttpResponseCode;

            try
            {
                string json = Encoding.UTF8.GetString(resp.Data);
                response.Result = JsonConvert.DeserializeObject<FeedbackDeleted>(json);
                customData.Add("json", json);
            }
            catch (Exception e)
            {
                Log.Error("CompareComplyService.OnDeleteFeedbackResponse()", "Exception: {0}", e.ToString());
                resp.Success = false;
            }

            if (((RequestObject<FeedbackDeleted>)req).Callback != null)
                ((RequestObject<FeedbackDeleted>)req).Callback(response, resp.Error, customData);
        }
        /// <summary>
        /// List a specified feedback entry.
        /// </summary>
        /// <param name="callback">The callback function that is invoked when the operation completes.</param>
        /// <param name="feedbackId">A string that specifies the feedback entry to be included in the output.</param>
        /// <param name="modelId">The analysis model to be used by the service. For the `/v1/element_classification` and
        /// `/v1/comparison` methods, the default is `contracts`. For the `/v1/tables` method, the default is `tables`.
        /// These defaults apply to the standalone methods as well as to the methods' use in batch-processing requests.
        /// (optional)</param>
        /// <param name="customData">A Dictionary<string, object> of data that will be passed to the callback. The raw
        /// json output from the REST call will be passed in this object as the value of the 'json'
        /// key.</string></param>
        /// <returns><see cref="GetFeedback" />GetFeedback</returns>
        public bool GetFeedback(Callback<GetFeedback> callback, string feedbackId, Dictionary<string, object> customData = null, string modelId = null)
        {
            if (callback == null)
                throw new ArgumentNullException("`callback` is required for `GetFeedback`");
            if (string.IsNullOrEmpty(feedbackId))
                throw new ArgumentNullException("`feedbackId` is required for `GetFeedback`");

            RequestObject<GetFeedback> req = new RequestObject<GetFeedback>
            {
                Callback = callback,
                HttpMethod = UnityWebRequest.kHttpVerbGET,
                DisableSslVerification = DisableSslVerification,
                CustomData = customData == null ? new Dictionary<string, object>() : customData
            };

            if (req.CustomData.ContainsKey(Constants.String.CUSTOM_REQUEST_HEADERS))
            {
                foreach (KeyValuePair<string, string> kvp in req.CustomData[Constants.String.CUSTOM_REQUEST_HEADERS] as Dictionary<string, string>)
                {
                    req.Headers.Add(kvp.Key, kvp.Value);
                }
            }

            foreach(KeyValuePair<string, string> kvp in Common.GetDefaultheaders("compare-comply", "V1", "GetFeedback"))
            {
                req.Headers.Add(kvp.Key, kvp.Value);
            }

            req.Parameters["version"] = VersionDate;
            if (!string.IsNullOrEmpty(modelId))
            {
                req.Parameters["model_id"] = modelId;
            }

            req.OnResponse = OnGetFeedbackResponse;

            RESTConnector connector = RESTConnector.GetConnector(Credentials, string.Format("/v1/feedback/{0}", feedbackId));
            if (connector == null)
            {
                return false;
            }

            return connector.Send(req);
        }

        private void OnGetFeedbackResponse(RESTConnector.Request req, RESTConnector.Response resp)
        {
            DetailedResponse<GetFeedback> response = new DetailedResponse<GetFeedback>();
            Dictionary<string, object> customData = ((RequestObject<GetFeedback>)req).CustomData;
            foreach (KeyValuePair<string, string> kvp in resp.Headers)
            {
                response.Headers.Add(kvp.Key, kvp.Value);
            }
            response.StatusCode = resp.HttpResponseCode;

            try
            {
                string json = Encoding.UTF8.GetString(resp.Data);
                response.Result = JsonConvert.DeserializeObject<GetFeedback>(json);
                customData.Add("json", json);
            }
            catch (Exception e)
            {
                Log.Error("CompareComplyService.OnGetFeedbackResponse()", "Exception: {0}", e.ToString());
                resp.Success = false;
            }

            if (((RequestObject<GetFeedback>)req).Callback != null)
                ((RequestObject<GetFeedback>)req).Callback(response, resp.Error, customData);
        }
        /// <summary>
        /// List the feedback in documents.
        /// </summary>
        /// <param name="callback">The callback function that is invoked when the operation completes.</param>
        /// <param name="feedbackType">An optional string that filters the output to include only feedback with the
        /// specified feedback type. The only permitted value is `element_classification`. (optional)</param>
        /// <param name="before">An optional string in the format `YYYY-MM-DD` that filters the output to include only
        /// feedback that was added before the specified date. (optional)</param>
        /// <param name="after">An optional string in the format `YYYY-MM-DD` that filters the output to include only
        /// feedback that was added after the specified date. (optional)</param>
        /// <param name="documentTitle">An optional string that filters the output to include only feedback from the
        /// document with the specified `document_title`. (optional)</param>
        /// <param name="modelId">An optional string that filters the output to include only feedback with the specified
        /// `model_id`. The only permitted value is `contracts`. (optional)</param>
        /// <param name="modelVersion">An optional string that filters the output to include only feedback with the
        /// specified `model_version`. (optional)</param>
        /// <param name="categoryRemoved">An optional string in the form of a comma-separated list of categories. If
        /// this is specified, the service filters the output to include only feedback that has at least one category
        /// from the list removed. (optional)</param>
        /// <param name="categoryAdded">An optional string in the form of a comma-separated list of categories. If this
        /// is specified, the service filters the output to include only feedback that has at least one category from
        /// the list added. (optional)</param>
        /// <param name="categoryNotChanged">An optional string in the form of a comma-separated list of categories. If
        /// this is specified, the service filters the output to include only feedback that has at least one category
        /// from the list unchanged. (optional)</param>
        /// <param name="typeRemoved">An optional string of comma-separated `nature`:`party` pairs. If this is
        /// specified, the service filters the output to include only feedback that has at least one `nature`:`party`
        /// pair from the list removed. (optional)</param>
        /// <param name="typeAdded">An optional string of comma-separated `nature`:`party` pairs. If this is specified,
        /// the service filters the output to include only feedback that has at least one `nature`:`party` pair from the
        /// list removed. (optional)</param>
        /// <param name="typeNotChanged">An optional string of comma-separated `nature`:`party` pairs. If this is
        /// specified, the service filters the output to include only feedback that has at least one `nature`:`party`
        /// pair from the list unchanged. (optional)</param>
        /// <param name="pageLimit">An optional integer specifying the number of documents that you want the service to
        /// return. (optional, default to 10)</param>
        /// <param name="cursor">An optional string that returns the set of documents after the previous set. Use this
        /// parameter with the `page_limit` parameter. (optional)</param>
        /// <param name="sort">An optional comma-separated list of fields in the document to sort on. You can optionally
        /// specify the sort direction by prefixing the value of the field with `-` for descending order or `+` for
        /// ascending order (the default). Currently permitted sorting fields are `created`, `user_id`, and
        /// `document_title`. (optional)</param>
        /// <param name="includeTotal">An optional boolean value. If specified as `true`, the `pagination` object in the
        /// output includes a value called `total` that gives the total count of feedback created. (optional)</param>
        /// <param name="customData">A Dictionary<string, object> of data that will be passed to the callback. The raw
        /// json output from the REST call will be passed in this object as the value of the 'json'
        /// key.</string></param>
        /// <returns><see cref="FeedbackList" />FeedbackList</returns>
        public bool ListFeedback(Callback<FeedbackList> callback, Dictionary<string, object> customData = null, string feedbackType = null, DateTime? before = null, DateTime? after = null, string documentTitle = null, string modelId = null, string modelVersion = null, string categoryRemoved = null, string categoryAdded = null, string categoryNotChanged = null, string typeRemoved = null, string typeAdded = null, string typeNotChanged = null, long? pageLimit = null, string cursor = null, string sort = null, bool? includeTotal = null)
        {
            if (callback == null)
                throw new ArgumentNullException("`callback` is required for `ListFeedback`");

            RequestObject<FeedbackList> req = new RequestObject<FeedbackList>
            {
                Callback = callback,
                HttpMethod = UnityWebRequest.kHttpVerbGET,
                DisableSslVerification = DisableSslVerification,
                CustomData = customData == null ? new Dictionary<string, object>() : customData
            };

            if (req.CustomData.ContainsKey(Constants.String.CUSTOM_REQUEST_HEADERS))
            {
                foreach (KeyValuePair<string, string> kvp in req.CustomData[Constants.String.CUSTOM_REQUEST_HEADERS] as Dictionary<string, string>)
                {
                    req.Headers.Add(kvp.Key, kvp.Value);
                }
            }

            foreach(KeyValuePair<string, string> kvp in Common.GetDefaultheaders("compare-comply", "V1", "ListFeedback"))
            {
                req.Headers.Add(kvp.Key, kvp.Value);
            }

            req.Parameters["version"] = VersionDate;
            if (!string.IsNullOrEmpty(feedbackType))
            {
                req.Parameters["feedback_type"] = feedbackType;
            }
            if (before != null)
            {
                req.Parameters["before"] = before;
            }
            if (after != null)
            {
                req.Parameters["after"] = after;
            }
            if (!string.IsNullOrEmpty(documentTitle))
            {
                req.Parameters["document_title"] = documentTitle;
            }
            if (!string.IsNullOrEmpty(modelId))
            {
                req.Parameters["model_id"] = modelId;
            }
            if (!string.IsNullOrEmpty(modelVersion))
            {
                req.Parameters["model_version"] = modelVersion;
            }
            if (!string.IsNullOrEmpty(categoryRemoved))
            {
                req.Parameters["category_removed"] = categoryRemoved;
            }
            if (!string.IsNullOrEmpty(categoryAdded))
            {
                req.Parameters["category_added"] = categoryAdded;
            }
            if (!string.IsNullOrEmpty(categoryNotChanged))
            {
                req.Parameters["category_not_changed"] = categoryNotChanged;
            }
            if (!string.IsNullOrEmpty(typeRemoved))
            {
                req.Parameters["type_removed"] = typeRemoved;
            }
            if (!string.IsNullOrEmpty(typeAdded))
            {
                req.Parameters["type_added"] = typeAdded;
            }
            if (!string.IsNullOrEmpty(typeNotChanged))
            {
                req.Parameters["type_not_changed"] = typeNotChanged;
            }
            if (pageLimit != null)
            {
                req.Parameters["page_limit"] = pageLimit;
            }
            if (!string.IsNullOrEmpty(cursor))
            {
                req.Parameters["cursor"] = cursor;
            }
            if (!string.IsNullOrEmpty(sort))
            {
                req.Parameters["sort"] = sort;
            }
            if (includeTotal != null)
            {
                req.Parameters["include_total"] = (bool)includeTotal ? "true" : "false";
            }

            req.OnResponse = OnListFeedbackResponse;

            RESTConnector connector = RESTConnector.GetConnector(Credentials, "/v1/feedback");
            if (connector == null)
            {
                return false;
            }

            return connector.Send(req);
        }

        private void OnListFeedbackResponse(RESTConnector.Request req, RESTConnector.Response resp)
        {
            DetailedResponse<FeedbackList> response = new DetailedResponse<FeedbackList>();
            Dictionary<string, object> customData = ((RequestObject<FeedbackList>)req).CustomData;
            foreach (KeyValuePair<string, string> kvp in resp.Headers)
            {
                response.Headers.Add(kvp.Key, kvp.Value);
            }
            response.StatusCode = resp.HttpResponseCode;

            try
            {
                string json = Encoding.UTF8.GetString(resp.Data);
                response.Result = JsonConvert.DeserializeObject<FeedbackList>(json);
                customData.Add("json", json);
            }
            catch (Exception e)
            {
                Log.Error("CompareComplyService.OnListFeedbackResponse()", "Exception: {0}", e.ToString());
                resp.Success = false;
            }

            if (((RequestObject<FeedbackList>)req).Callback != null)
                ((RequestObject<FeedbackList>)req).Callback(response, resp.Error, customData);
        }
        /// <summary>
        /// Submit a batch-processing request.
        ///
        /// Run Compare and Comply methods over a collection of input documents.
        /// **Important:** Batch processing requires the use of the [IBM Cloud Object Storage
        /// service](https://cloud.ibm.com/docs/services/cloud-object-storage/about-cos.html#about-ibm-cloud-object-storage).
        /// The use of IBM Cloud Object Storage with Compare and Comply is discussed at [Using batch
        /// processing](https://cloud.ibm.com/docs/services/compare-comply/batching.html#before-you-batch).
        /// </summary>
        /// <param name="callback">The callback function that is invoked when the operation completes.</param>
        /// <param name="function">The Compare and Comply method to run across the submitted input documents.</param>
        /// <param name="inputCredentialsFile">A JSON file containing the input Cloud Object Storage credentials. At a
        /// minimum, the credentials must enable `READ` permissions on the bucket defined by the `input_bucket_name`
        /// parameter.</param>
        /// <param name="inputBucketLocation">The geographical location of the Cloud Object Storage input bucket as
        /// listed on the **Endpoint** tab of your Cloud Object Storage instance; for example, `us-geo`, `eu-geo`, or
        /// `ap-geo`.</param>
        /// <param name="inputBucketName">The name of the Cloud Object Storage input bucket.</param>
        /// <param name="outputCredentialsFile">A JSON file that lists the Cloud Object Storage output credentials. At a
        /// minimum, the credentials must enable `READ` and `WRITE` permissions on the bucket defined by the
        /// `output_bucket_name` parameter.</param>
        /// <param name="outputBucketLocation">The geographical location of the Cloud Object Storage output bucket as
        /// listed on the **Endpoint** tab of your Cloud Object Storage instance; for example, `us-geo`, `eu-geo`, or
        /// `ap-geo`.</param>
        /// <param name="outputBucketName">The name of the Cloud Object Storage output bucket.</param>
        /// <param name="modelId">The analysis model to be used by the service. For the `/v1/element_classification` and
        /// `/v1/comparison` methods, the default is `contracts`. For the `/v1/tables` method, the default is `tables`.
        /// These defaults apply to the standalone methods as well as to the methods' use in batch-processing requests.
        /// (optional)</param>
        /// <param name="customData">A Dictionary<string, object> of data that will be passed to the callback. The raw
        /// json output from the REST call will be passed in this object as the value of the 'json'
        /// key.</string></param>
        /// <returns><see cref="BatchStatus" />BatchStatus</returns>
        public bool CreateBatch(Callback<BatchStatus> callback, string function, System.IO.FileStream inputCredentialsFile, string inputBucketLocation, string inputBucketName, System.IO.FileStream outputCredentialsFile, string outputBucketLocation, string outputBucketName, Dictionary<string, object> customData = null, string modelId = null)
        {
            if (callback == null)
                throw new ArgumentNullException("`callback` is required for `CreateBatch`");
            if (string.IsNullOrEmpty(function))
                throw new ArgumentNullException("`function` is required for `CreateBatch`");
            if (inputCredentialsFile == null)
                throw new ArgumentNullException("`inputCredentialsFile` is required for `CreateBatch`");
            if (string.IsNullOrEmpty(inputBucketLocation))
                throw new ArgumentNullException("`inputBucketLocation` is required for `CreateBatch`");
            if (string.IsNullOrEmpty(inputBucketName))
                throw new ArgumentNullException("`inputBucketName` is required for `CreateBatch`");
            if (outputCredentialsFile == null)
                throw new ArgumentNullException("`outputCredentialsFile` is required for `CreateBatch`");
            if (string.IsNullOrEmpty(outputBucketLocation))
                throw new ArgumentNullException("`outputBucketLocation` is required for `CreateBatch`");
            if (string.IsNullOrEmpty(outputBucketName))
                throw new ArgumentNullException("`outputBucketName` is required for `CreateBatch`");

            RequestObject<BatchStatus> req = new RequestObject<BatchStatus>
            {
                Callback = callback,
                HttpMethod = UnityWebRequest.kHttpVerbPOST,
                DisableSslVerification = DisableSslVerification,
                CustomData = customData == null ? new Dictionary<string, object>() : customData
            };

            if (req.CustomData.ContainsKey(Constants.String.CUSTOM_REQUEST_HEADERS))
            {
                foreach (KeyValuePair<string, string> kvp in req.CustomData[Constants.String.CUSTOM_REQUEST_HEADERS] as Dictionary<string, string>)
                {
                    req.Headers.Add(kvp.Key, kvp.Value);
                }
            }

            foreach(KeyValuePair<string, string> kvp in Common.GetDefaultheaders("compare-comply", "V1", "CreateBatch"))
            {
                req.Headers.Add(kvp.Key, kvp.Value);
            }

            req.Parameters["version"] = VersionDate;
            req.Forms = new Dictionary<string, RESTConnector.Form>();
            req.Forms["input_credentials_file"] = new RESTConnector.Form(inputCredentialsFile, inputCredentialsFile.Name, "application/json");
            if (!string.IsNullOrEmpty(inputBucketLocation))
            {
                req.Forms["input_bucket_location"] = new RESTConnector.Form(inputBucketLocation);
            }
            if (!string.IsNullOrEmpty(inputBucketName))
            {
                req.Forms["input_bucket_name"] = new RESTConnector.Form(inputBucketName);
            }
            req.Forms["output_credentials_file"] = new RESTConnector.Form(outputCredentialsFile, outputCredentialsFile.Name, "application/json");
            if (!string.IsNullOrEmpty(outputBucketLocation))
            {
                req.Forms["output_bucket_location"] = new RESTConnector.Form(outputBucketLocation);
            }
            if (!string.IsNullOrEmpty(outputBucketName))
            {
                req.Forms["output_bucket_name"] = new RESTConnector.Form(outputBucketName);
            }
            if (!string.IsNullOrEmpty(function))
            {
                req.Parameters["function"] = function;
            }
            if (!string.IsNullOrEmpty(modelId))
            {
                req.Parameters["model_id"] = modelId;
            }

            req.OnResponse = OnCreateBatchResponse;

            RESTConnector connector = RESTConnector.GetConnector(Credentials, "/v1/batches");
            if (connector == null)
            {
                return false;
            }

            return connector.Send(req);
        }

        private void OnCreateBatchResponse(RESTConnector.Request req, RESTConnector.Response resp)
        {
            DetailedResponse<BatchStatus> response = new DetailedResponse<BatchStatus>();
            Dictionary<string, object> customData = ((RequestObject<BatchStatus>)req).CustomData;
            foreach (KeyValuePair<string, string> kvp in resp.Headers)
            {
                response.Headers.Add(kvp.Key, kvp.Value);
            }
            response.StatusCode = resp.HttpResponseCode;

            try
            {
                string json = Encoding.UTF8.GetString(resp.Data);
                response.Result = JsonConvert.DeserializeObject<BatchStatus>(json);
                customData.Add("json", json);
            }
            catch (Exception e)
            {
                Log.Error("CompareComplyService.OnCreateBatchResponse()", "Exception: {0}", e.ToString());
                resp.Success = false;
            }

            if (((RequestObject<BatchStatus>)req).Callback != null)
                ((RequestObject<BatchStatus>)req).Callback(response, resp.Error, customData);
        }
        /// <summary>
        /// Get information about a specific batch-processing request.
        ///
        /// Get information about a batch-processing request with a specified ID.
        /// </summary>
        /// <param name="callback">The callback function that is invoked when the operation completes.</param>
        /// <param name="batchId">The ID of the batch-processing request whose information you want to retrieve.</param>
        /// <param name="customData">A Dictionary<string, object> of data that will be passed to the callback. The raw
        /// json output from the REST call will be passed in this object as the value of the 'json'
        /// key.</string></param>
        /// <returns><see cref="BatchStatus" />BatchStatus</returns>
        public bool GetBatch(Callback<BatchStatus> callback, string batchId, Dictionary<string, object> customData = null)
        {
            if (callback == null)
                throw new ArgumentNullException("`callback` is required for `GetBatch`");
            if (string.IsNullOrEmpty(batchId))
                throw new ArgumentNullException("`batchId` is required for `GetBatch`");

            RequestObject<BatchStatus> req = new RequestObject<BatchStatus>
            {
                Callback = callback,
                HttpMethod = UnityWebRequest.kHttpVerbGET,
                DisableSslVerification = DisableSslVerification,
                CustomData = customData == null ? new Dictionary<string, object>() : customData
            };

            if (req.CustomData.ContainsKey(Constants.String.CUSTOM_REQUEST_HEADERS))
            {
                foreach (KeyValuePair<string, string> kvp in req.CustomData[Constants.String.CUSTOM_REQUEST_HEADERS] as Dictionary<string, string>)
                {
                    req.Headers.Add(kvp.Key, kvp.Value);
                }
            }

            foreach(KeyValuePair<string, string> kvp in Common.GetDefaultheaders("compare-comply", "V1", "GetBatch"))
            {
                req.Headers.Add(kvp.Key, kvp.Value);
            }

            req.Parameters["version"] = VersionDate;

            req.OnResponse = OnGetBatchResponse;

            RESTConnector connector = RESTConnector.GetConnector(Credentials, string.Format("/v1/batches/{0}", batchId));
            if (connector == null)
            {
                return false;
            }

            return connector.Send(req);
        }

        private void OnGetBatchResponse(RESTConnector.Request req, RESTConnector.Response resp)
        {
            DetailedResponse<BatchStatus> response = new DetailedResponse<BatchStatus>();
            Dictionary<string, object> customData = ((RequestObject<BatchStatus>)req).CustomData;
            foreach (KeyValuePair<string, string> kvp in resp.Headers)
            {
                response.Headers.Add(kvp.Key, kvp.Value);
            }
            response.StatusCode = resp.HttpResponseCode;

            try
            {
                string json = Encoding.UTF8.GetString(resp.Data);
                response.Result = JsonConvert.DeserializeObject<BatchStatus>(json);
                customData.Add("json", json);
            }
            catch (Exception e)
            {
                Log.Error("CompareComplyService.OnGetBatchResponse()", "Exception: {0}", e.ToString());
                resp.Success = false;
            }

            if (((RequestObject<BatchStatus>)req).Callback != null)
                ((RequestObject<BatchStatus>)req).Callback(response, resp.Error, customData);
        }
        /// <summary>
        /// List submitted batch-processing jobs.
        ///
        /// List the batch-processing jobs submitted by users.
        /// </summary>
        /// <param name="callback">The callback function that is invoked when the operation completes.</param>
        /// <param name="customData">A Dictionary<string, object> of data that will be passed to the callback. The raw
        /// json output from the REST call will be passed in this object as the value of the 'json'
        /// key.</string></param>
        /// <returns><see cref="Batches" />Batches</returns>
        public bool ListBatches(Callback<Batches> callback, Dictionary<string, object> customData = null)
        {
            if (callback == null)
                throw new ArgumentNullException("`callback` is required for `ListBatches`");

            RequestObject<Batches> req = new RequestObject<Batches>
            {
                Callback = callback,
                HttpMethod = UnityWebRequest.kHttpVerbGET,
                DisableSslVerification = DisableSslVerification,
                CustomData = customData == null ? new Dictionary<string, object>() : customData
            };

            if (req.CustomData.ContainsKey(Constants.String.CUSTOM_REQUEST_HEADERS))
            {
                foreach (KeyValuePair<string, string> kvp in req.CustomData[Constants.String.CUSTOM_REQUEST_HEADERS] as Dictionary<string, string>)
                {
                    req.Headers.Add(kvp.Key, kvp.Value);
                }
            }

            foreach(KeyValuePair<string, string> kvp in Common.GetDefaultheaders("compare-comply", "V1", "ListBatches"))
            {
                req.Headers.Add(kvp.Key, kvp.Value);
            }

            req.Parameters["version"] = VersionDate;

            req.OnResponse = OnListBatchesResponse;

            RESTConnector connector = RESTConnector.GetConnector(Credentials, "/v1/batches");
            if (connector == null)
            {
                return false;
            }

            return connector.Send(req);
        }

        private void OnListBatchesResponse(RESTConnector.Request req, RESTConnector.Response resp)
        {
            DetailedResponse<Batches> response = new DetailedResponse<Batches>();
            Dictionary<string, object> customData = ((RequestObject<Batches>)req).CustomData;
            foreach (KeyValuePair<string, string> kvp in resp.Headers)
            {
                response.Headers.Add(kvp.Key, kvp.Value);
            }
            response.StatusCode = resp.HttpResponseCode;

            try
            {
                string json = Encoding.UTF8.GetString(resp.Data);
                response.Result = JsonConvert.DeserializeObject<Batches>(json);
                customData.Add("json", json);
            }
            catch (Exception e)
            {
                Log.Error("CompareComplyService.OnListBatchesResponse()", "Exception: {0}", e.ToString());
                resp.Success = false;
            }

            if (((RequestObject<Batches>)req).Callback != null)
                ((RequestObject<Batches>)req).Callback(response, resp.Error, customData);
        }
        /// <summary>
        /// Update a pending or active batch-processing request.
        ///
        /// Update a pending or active batch-processing request. You can rescan the input bucket to check for new
        /// documents or cancel a request.
        /// </summary>
        /// <param name="callback">The callback function that is invoked when the operation completes.</param>
        /// <param name="batchId">The ID of the batch-processing request you want to update.</param>
        /// <param name="action">The action you want to perform on the specified batch-processing request.</param>
        /// <param name="modelId">The analysis model to be used by the service. For the `/v1/element_classification` and
        /// `/v1/comparison` methods, the default is `contracts`. For the `/v1/tables` method, the default is `tables`.
        /// These defaults apply to the standalone methods as well as to the methods' use in batch-processing requests.
        /// (optional)</param>
        /// <param name="customData">A Dictionary<string, object> of data that will be passed to the callback. The raw
        /// json output from the REST call will be passed in this object as the value of the 'json'
        /// key.</string></param>
        /// <returns><see cref="BatchStatus" />BatchStatus</returns>
        public bool UpdateBatch(Callback<BatchStatus> callback, string batchId, string action, Dictionary<string, object> customData = null, string modelId = null)
        {
            if (callback == null)
                throw new ArgumentNullException("`callback` is required for `UpdateBatch`");
            if (string.IsNullOrEmpty(batchId))
                throw new ArgumentNullException("`batchId` is required for `UpdateBatch`");
            if (string.IsNullOrEmpty(action))
                throw new ArgumentNullException("`action` is required for `UpdateBatch`");

            RequestObject<BatchStatus> req = new RequestObject<BatchStatus>
            {
                Callback = callback,
                HttpMethod = UnityWebRequest.kHttpVerbPUT,
                DisableSslVerification = DisableSslVerification,
                CustomData = customData == null ? new Dictionary<string, object>() : customData
            };

            if (req.CustomData.ContainsKey(Constants.String.CUSTOM_REQUEST_HEADERS))
            {
                foreach (KeyValuePair<string, string> kvp in req.CustomData[Constants.String.CUSTOM_REQUEST_HEADERS] as Dictionary<string, string>)
                {
                    req.Headers.Add(kvp.Key, kvp.Value);
                }
            }

            foreach(KeyValuePair<string, string> kvp in Common.GetDefaultheaders("compare-comply", "V1", "UpdateBatch"))
            {
                req.Headers.Add(kvp.Key, kvp.Value);
            }

            req.Parameters["version"] = VersionDate;
            if (!string.IsNullOrEmpty(action))
            {
                req.Parameters["action"] = action;
            }
            if (!string.IsNullOrEmpty(modelId))
            {
                req.Parameters["model_id"] = modelId;
            }

            req.OnResponse = OnUpdateBatchResponse;

            RESTConnector connector = RESTConnector.GetConnector(Credentials, string.Format("/v1/batches/{0}", batchId));
            if (connector == null)
            {
                return false;
            }

            return connector.Send(req);
        }

        private void OnUpdateBatchResponse(RESTConnector.Request req, RESTConnector.Response resp)
        {
            DetailedResponse<BatchStatus> response = new DetailedResponse<BatchStatus>();
            Dictionary<string, object> customData = ((RequestObject<BatchStatus>)req).CustomData;
            foreach (KeyValuePair<string, string> kvp in resp.Headers)
            {
                response.Headers.Add(kvp.Key, kvp.Value);
            }
            response.StatusCode = resp.HttpResponseCode;

            try
            {
                string json = Encoding.UTF8.GetString(resp.Data);
                response.Result = JsonConvert.DeserializeObject<BatchStatus>(json);
                customData.Add("json", json);
            }
            catch (Exception e)
            {
                Log.Error("CompareComplyService.OnUpdateBatchResponse()", "Exception: {0}", e.ToString());
                resp.Success = false;
            }

            if (((RequestObject<BatchStatus>)req).Callback != null)
                ((RequestObject<BatchStatus>)req).Callback(response, resp.Error, customData);
        }
    }
}