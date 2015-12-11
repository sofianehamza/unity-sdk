﻿/**
* Copyright 2015 IBM Corp. All Rights Reserved.
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

using System.Collections;
using System.Collections.Generic;
using IBM.Watson.Logging;
using UnityEngine;
using FullSerializer;
using System.IO;
using MiniJSON;
using System;

namespace IBM.Watson.Utilities
{
    /// <summary>
    /// This class is used to hold configuration data for SDK.
    /// </summary>
    public class Config
    {
        /// <summary>
        /// Serialized class for holding the user credentials for a service.
        /// </summary>
        public class CredentialInfo
        {
            /// <summary>
            /// The ID of the service this is the credentials.
            /// </summary>
            public string m_ServiceID;
            /// <summary>
            /// The URL for these credentials.
            /// </summary>
            public string m_URL;
            /// <summary>
            /// The user name for these credentials.
            /// </summary>
            public string m_User;
            /// <summary>
            /// The password for these credentials.
            /// </summary>
            public string m_Password;

            /// <summary>
            /// Generate JSON credentials.
            /// </summary>
            /// <returns>Returns a string of the JSON.</returns>
            public string MakeJSON()
            {
                return "{\n\t\"credentials\": {\n\t\t\"url\": \"" + m_URL + "\",\n\t\t\"username\": \"" + m_User + "\",\n\t\t\"password\": \"" + m_Password + "\"\n\t}\n}";
            }

            /// <summary>
            /// Parses a BlueMix json credentials into this object.
            /// </summary>
            /// <param name="json">The JSON data to parse.</param>
            /// <returns>Returns true on success.</returns>
            public bool ParseJSON( string json )
            {
                try {
                    IDictionary iParse = Json.Deserialize( json ) as IDictionary;
                    IDictionary iCredentials = iParse["credentials"] as IDictionary;
                    m_URL = (string)iCredentials["url"];
                    m_User = (string)iCredentials["username"];
                    m_Password = (string)iCredentials["password"];

                    return true;
                }
                catch( Exception e )
                {
                    Log.Error( "Config", "Caught Exception: {0}", e.ToString() );
                }

                return false;
            }
        }

        #region Private Data
        [fsProperty]
        private float m_TimeOut = 30.0f;
        [fsProperty]
        private int m_MaxRestConnections = 5;
        [fsProperty]
        private bool m_EnableGateway = false;
        [fsProperty]
        private string m_GatewayURL = ""; //"https://9.53.162.55:9443/webApp";
        [fsProperty]
        private string m_ProductKey = null;
        [fsProperty]
        private List<CredentialInfo> m_Credentials = new List<CredentialInfo>();

        private static fsSerializer sm_Serializer = new fsSerializer();
        #endregion

        #region Public Properties
        /// <summary>
        /// Returns true if the configuration is loaded or not.
        /// </summary>
        [fsIgnore]
        public bool ConfigLoaded { get; private set; }
        /// <summary>
        /// Returns the singleton instance.
        /// </summary>
        public static Config Instance { get { return Singleton<Config>.Instance; } }
        /// <summary>
        /// Returns the Timeout for requests made to the server.
        /// </summary>
        public float TimeOut { get { return m_TimeOut; } set { m_TimeOut = value; } }
        /// <summary>
        /// Maximum number of connections Watson will make to the server back-end at any one time.
        /// </summary>
        public int MaxRestConnections { get { return m_MaxRestConnections; } set { m_MaxRestConnections = value; } }
        /// <summary>
        /// Returns the list of credentials used to login to the various services.
        /// </summary>
        public List<CredentialInfo> Credentials { get { return m_Credentials; } set { m_Credentials = value; } }
        /// <summary>
        /// Enable the gateway usage.
        /// </summary>
        public bool EnableGateway { get { return m_EnableGateway; } set { m_EnableGateway = value; } }
        /// <summary>
        /// The URL of the gateway to use.
        /// </summary>
        public string GatewayURL { get { return m_GatewayURL; } set { m_GatewayURL = value; } }
        /// <summary>
        /// The product key used to communicate with the gateway.
        /// </summary>
        public string ProductKey { get { return m_ProductKey; } set { m_ProductKey = value; } }

        #endregion

        /// <summary>
        /// Default constructor will call LoadConfig() automatically.
        /// </summary>
        public Config()
        {
            LoadConfig();
        }

        /// <summary>
        /// Find BlueMix credentials by the service ID.
        /// </summary>
        /// <param name="serviceID">The ID of the service to find.</param>
        /// <returns>Returns null if the credentials cannot be found.</returns>
        public CredentialInfo FindCredentials( string serviceID )
        {
            foreach( var info in m_Credentials )
                if ( info.m_ServiceID == serviceID )
                    return info;
            return null;
        }

        /// <summary>
        /// Invoking this function will start the co-routine to load the configuration. The user should check the 
        /// ConfigLoaded property to check when the configuration is actually loaded.
        /// </summary>
        public void LoadConfig()
        {
#if !UNITY_ANDROID || UNITY_EDITOR
            try {
                if (! Directory.Exists( Application.streamingAssetsPath ) )
                    Directory.CreateDirectory( Application.streamingAssetsPath );
				LoadConfig( System.IO.File.ReadAllText( Application.streamingAssetsPath + Constants.Path.CONFIG_FILE ) );
            }
            catch( System.IO.FileNotFoundException )
            {}
#else
            Runnable.Run(LoadConfigCR());
#endif
        }

        /// <summary>
        /// Load the configuration from the given JSON data.
        /// </summary>
        /// <param name="json">The string containing the configuration JSON data.</param>
        /// <returns></returns>
        public bool LoadConfig(string json)
        {
            fsData data = null;
            fsResult r = fsJsonParser.Parse(json, out data);
            if (!r.Succeeded)
            {
                Log.Error("Config", "Failed to parse Config.json: {0}", r.ToString());
                return false;
            }

            object obj = this;
            r = sm_Serializer.TryDeserialize(data, GetType(), ref obj);
            if (!r.Succeeded)
            {
                Log.Error("Config", "Failed to parse Config.json: {0}", r.ToString());
                return false;
            }

            ConfigLoaded = true;
            return true;
        }

        /// <summary>
        /// Save this COnfig into JSON.
        /// </summary>
        /// <param name="pretty">If true, then the json data will be formatted for readability.</param>
        /// <returns></returns>
        public string SaveConfig(bool pretty = true)
        {
            fsData data = null;
            sm_Serializer.TrySerialize(GetType(), this, out data);

            if (!System.IO.Directory.Exists(Application.streamingAssetsPath))
                System.IO.Directory.CreateDirectory(Application.streamingAssetsPath);

            if (pretty)
                return fsJsonPrinter.PrettyJson(data);

            return fsJsonPrinter.CompressedJson(data);
        }

        private IEnumerator LoadConfigCR()
        {
            // load the config using WWW, since this works on all platforms..
            WWW request = new WWW(Application.streamingAssetsPath + Constants.Path.CONFIG_FILE);
            while (!request.isDone)
                yield return null;

            LoadConfig(request.text);
            yield break;
        }



    }
}
