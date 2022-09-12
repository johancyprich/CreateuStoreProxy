/// APPLICATION: CreateuStoreProxy
/// VERSION: 1.1.0
/// DATE: June 11, 2018
/// AUTHOR: Johan Cyprich
/// AUTHOR EMAIL: jcyprich@live.com
///
/// LICENSE:
/// The MIT License (MIT)
///
/// Copyright (c) 2014 Johan Cyprich. All rights reserved.
///
/// Permission is hereby granted, free of charge, to any person obtaining a copy
/// of this software and associated documentation files (the "Software"), to deal
/// in the Software without restriction, including without limitation the rights
/// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
/// copies of the Software, and to permit persons to whom the Software is
/// furnished to do so, subject to the following conditions:
///
/// The above copyright notice and this permission notice shall be included in
/// all copies or substantial portions of the Software.
///
/// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
/// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
/// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
/// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
/// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
/// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
/// THE SOFTWARE.
///
/// SUMMARY:
/// Create the httpd.conf file in proxy server for XMPie uStore. The file is written on
/// local folder and then copied to the proxy server path defined by _outputPath.


// Used to give a ReadKey prompt after app installs new .conf file if app is run from
// the Windows desktop. Comment this out if run in command line mode.
#define DESKTOP


using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;


namespace CreateuStoreProxy
{
	class Program
	{
		public static string connectionString;

		private static string _dbHost;                                           // database server hosting uStore
		private static string _dbDatabase;                                       // uStore database
		private static string _dbUser;                                           // user name for uStore database
		private static string _dbPassword;                                       // password for uStore database

		private static string _storeURL;                                         // URL for uStore, assuming all are the same

		private static string _helioconName;
		private static string _helioconCode;

		private static string _outputPath;                                       // path to httpd.conf on Heliocon folder in the uStore server
		private static string _appFolder = Directory.GetCurrentDirectory ();     // path to this applications's folder


		///////////////////////////////////////////////////////////////////////////////////////////////
		/// SUMMARY:
		/// Read the settings in CreateuStoreProxy.Settings.xml.
		///
		/// GLOBAL VARIABLES:
		/// Values retrieved for the following global variables from the settings file:
		///   _dbHost
		///   _dbDatabase
		///   _dbUser
		///   _dbPassord
		///   _helioconName
		///   _helioconCode
		///   _outputPath
		///////////////////////////////////////////////////////////////////////////////////////////////

		static protected void ReadSettings ()
		{
			string element = "";
			XmlTextReader reader = new XmlTextReader (_appFolder + @"\CreateuStoreProxy.Settings.xml");

			while (reader.Read ())
			{
				switch (reader.NodeType)
				{
					case XmlNodeType.Element:
						element = reader.Name;
						break;

					case XmlNodeType.Text:
						if (element == "Host")
							_dbHost = reader.Value;

						else if (element == "Database")
							_dbDatabase = reader.Value;

						else if (element == "User")
							_dbUser = reader.Value;

						else if (element == "Password")
							_dbPassword = reader.Value;

						else if (element == "RegistrationName")
							_helioconName = reader.Value;

						else if (element == "RegistrationCode")
							_helioconCode = reader.Value;

						else if (element == "Output")
							_outputPath = reader.Value;

						break;
				} // switch (reader.NodeType)
			} // while (reader.Read ())
		} // static protected void ReadSettings ()


		///////////////////////////////////////////////////////////////////////////////////////////////
		/// SUMMARY:
		/// Write the final block of data for httpd.conf.
		///
		/// GLOBAL VARIABLES:
		///   _storeURL
		///////////////////////////////////////////////////////////////////////////////////////////////

		static protected void WriteEndConf ()
		{
			using (StreamWriter w = new StreamWriter (_appFolder + "\\httpd.conf", true))
			{
				w.WriteLine ("####USTORE_PROXY_STORE_FOLDER_SECTION_END########USTORE_PROXY_FINALIZE_SECTION_BEGIN####");
				w.WriteLine ("#General rules to access uStore and uStore.CommonControls");
				w.WriteLine ("");
				w.WriteLine ("");
				w.WriteLine ("");
				w.WriteLine ("RewriteCond %{HTTPS} off");
				w.WriteLine ("RewriteCond %{HTTP_Host} ^" + _storeURL + "$");
				w.WriteLine (@"RewriteProxy  /aspnet_client/(.*) http\://" + _storeURL + "/aspnet_client/$1 [NC,U,L]");
				w.WriteLine ("");
				w.WriteLine ("");
				w.WriteLine ("RewriteCond %{HTTPS} off");
				w.WriteLine ("RewriteCond %{HTTP_Host} ^" + _storeURL + "$");
				w.WriteLine (@"RewriteProxy  ^/ustore((\.|/).*)?$ http\://" + _storeURL + "/ustore$1 [NC,U,L]");
				w.WriteLine ("");
				w.WriteLine ("");
				w.WriteLine ("#--------------------------------------#");
				w.WriteLine ("# uStore Proxy Shared - SSL");
				w.WriteLine ("#--------------------------------------#");
				w.WriteLine ("");
				w.WriteLine ("");
				w.WriteLine ("RewriteCond %{HTTPS} on");
				w.WriteLine ("RewriteCond %{HTTP_Host} ^" + _storeURL + "$");
				w.WriteLine (@"RewriteProxy  /aspnet_client/(.*) https\://" + _storeURL + "/aspnet_client/$1 [NC,U,L]");
				w.WriteLine ("");
				w.WriteLine ("");
				w.WriteLine ("RewriteCond %{HTTPS} on");
				w.WriteLine ("RewriteCond %{HTTP_Host} ^" + _storeURL + "$");
				w.WriteLine (@"RewriteProxy  ^/ustore((\.|/).*)?$ https\://" + _storeURL + "/ustore$1 [NC,U,L]");
				w.WriteLine ("");
				w.WriteLine ("");
				w.WriteLine ("####USTORE_PROXY_FINALIZE_SECTION_END####");
				w.WriteLine ("####USTORE_END####");
			}
		} // static protected void WriteEndConf ()


		///////////////////////////////////////////////////////////////////////////////////////////////
		/// SUMMARY:
		/// Write the starting block of data for httpd.conf.
		///
		/// GLOBAL VARIABLES:
		///   _appFolder
		///   _helioconName
		///   _helioconCode
		///////////////////////////////////////////////////////////////////////////////////////////////

		static protected void WriteStartConf ()
		{
			using (StreamWriter w = new StreamWriter (_appFolder + @"\httpd.conf"))
			{
				w.WriteLine ("# Helicon ISAPI_Rewrite configuration file");
				w.WriteLine ("# Version 3.1.0.99");
				w.WriteLine ("");
				w.WriteLine ("");
				w.WriteLine ("# Registration info");
				w.WriteLine ("RegistrationName= " + _helioconName);
				w.WriteLine ("RegistrationCode= " + _helioconCode);
				w.WriteLine ("####USTORE_BEGIN####");
				w.WriteLine ("");
				w.WriteLine ("");
			}
		} // static protected void WriteStartConf ()


		///////////////////////////////////////////////////////////////////////////////////////////////
		/// SUMMARY:
		/// Builds the redirect code for the httpd.conf by reading information from the Store folder
		/// on the uStore database.
		///
		/// GLOBAL VARIABLES:
		///   _storeURL
		///////////////////////////////////////////////////////////////////////////////////////////////

		static protected void WriteStoresConf ()
		{
			using (StreamWriter w = new StreamWriter (_appFolder + @"\httpd.conf", true))
			{
				w.WriteLine ("# Stores");

				int storeID = 0;                                  // StoreID from Store table
				string landingFolder = "";                        // LandingFolder from Store table

				using (SqlConnection conn = new SqlConnection (connectionString))
				{
					conn.Open ();

					string sql = "SELECT StoreID, LandingDomain, LandingFolder, StatusID"
							   + "  FROM Store"
							   + "  WHERE StatusID=1";

					using (SqlCommand command = new SqlCommand (sql, conn))
					using (SqlDataReader reader = command.ExecuteReader ())
					{
						while (reader.Read ())
						{
							storeID = reader.GetInt32 (0);
							_storeURL = reader.GetString (1);                               // setting global variable, hopefully all URLs are the same
							landingFolder = reader.GetString (2);

							Console.WriteLine ($"Store ID: {storeID} :: {landingFolder}");

							w.WriteLine ("####USTORE_PROXY_STORE_FOLDER_SECTION_BEGIN####");
							w.WriteLine ("####STORE ID: " + storeID + "_BEGIN####");
							w.WriteLine ("");
							w.WriteLine ("");
							w.WriteLine ("RewriteCond %{HTTPS} off");
							w.WriteLine ("RewriteCond %{HTTP_Host} ^" + _storeURL + "$");
							w.WriteLine ("RewriteProxy  ^/" + landingFolder + @"(/)?$ http\://" + _storeURL + @"/ustore/default.aspx\?storeid=" + storeID + " [NC,U,L]");
							w.WriteLine ("");
							w.WriteLine ("");
							w.WriteLine ("RewriteCond %{HTTPS} off");
							w.WriteLine ("RewriteCond %{HTTP_Host} ^" + _storeURL + "$");
							w.WriteLine ("RewriteProxy  ^/" + landingFolder + @"/(.*)$ http\://" + _storeURL + "/uStore/$1 [NC,U,L]");
							w.WriteLine ("");
							w.WriteLine ("");
							w.WriteLine ("#--------------------------------------#");
							w.WriteLine ("# uStore Proxy store with folder - SSL");
							w.WriteLine ("#--------------------------------------#");
							w.WriteLine ("");
							w.WriteLine ("");
							w.WriteLine ("RewriteCond %{HTTPS} on");
							w.WriteLine ("RewriteCond %{HTTP_Host} ^" + _storeURL + "$");
							w.WriteLine ("RewriteProxy  ^/" + landingFolder + @"(/)?$ https\://" + _storeURL + @"/ustore/default.aspx\?storeid=" + storeID + " [NC,U,L]");
							w.WriteLine ("");
							w.WriteLine ("");
							w.WriteLine ("RewriteCond %{HTTPS} on");
							w.WriteLine ("RewriteCond %{HTTP_Host} ^" + _storeURL + "$");
							w.WriteLine ("RewriteProxy  ^/" + landingFolder + @"/(.*)$ https\://" + _storeURL + "/ustore/$1 [NC,U,L]");
							w.WriteLine ("");
							w.WriteLine ("");
							w.WriteLine ("####STORE ID: " + storeID + "_END####");
							w.WriteLine ("####USTORE_PROXY_STORE_FOLDER_SECTION_END####");
							w.WriteLine ("");
							w.WriteLine ("");
						} // while (reader.Read ())
					} // using (SqlDataReader reader = command.ExecuteReader ())

					conn.Close ();
				} // using (SqlConnection conn = new SqlConnection (connectionString))
			}

			Console.WriteLine ();
		} // static protected void WriteStoresConf ()


		///////////////////////////////////////////////////////////////////////////////////////////////
		/// SUMMARY:
		/// Copy the httpd.conf created in the application folder to the uStore server. The existing
		/// .conf file will be renamed before the new file is copied to the folder. The old file will
		/// have the date (YYYYMMDD-HHMMSS) appended to its filename.
		///
		/// GLOBAL VARIABLES:
		///   _appFolder
		///   _outputPath
		///////////////////////////////////////////////////////////////////////////////////////////////

		static protected void CopyConf ()
		{
			if (File.Exists (_outputPath + @"\httpd.conf"))
			{
				DateTime nowDate = DateTime.Now;               // get the current date

				// Rename active httpd.conf to httpd.conf.YYYYMMDD

				File.Move (_outputPath + @"\httpd.conf",
						   _outputPath + @"\httpd.conf." + nowDate.Year.ToString () + nowDate.Month.ToString ("00") + nowDate.Day.ToString ("00") + "-"
														 + nowDate.Hour.ToString ("00") + nowDate.Minute.ToString ("00") + nowDate.Second.ToString ("00"));

				// Copy new httpd.conf to uStore Proxy server.

				File.Copy (_appFolder + @"\httpd.conf", _outputPath + @"\httpd.conf");
			}
		} // static protected void CopyConf ()


		/*** MAIN *******************************************************************************************************************/


		static void Main (string [] args)
		{
			Console.WriteLine ("Create uStore Proxy 1.1.0");
			Console.WriteLine ("by Johan Cyprich");
			Console.WriteLine ("Copyright (C) 2014-2018 Johan Cyprich. All rights reserved.");
			Console.WriteLine ("Licensed under the MIT License.");
			Console.WriteLine ("");

			ReadSettings ();

			connectionString = "Data Source=" + _dbHost + ";"
							 + "Initial Catalog=" + _dbDatabase + ";"
							 + "User id=" + _dbUser + ";"
							 + "Password=" + _dbPassword;

			WriteStartConf ();
			WriteStoresConf ();
			WriteEndConf ();

			if (_outputPath != "")
				CopyConf ();

			Console.WriteLine ("httpd.conf generated");

#if DESKTOP
			Console.ReadKey ();
#endif
		} // static void Main (string [] args)

	} // class Program
} // namespace CreateuStoreProxy
