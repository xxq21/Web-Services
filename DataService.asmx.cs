#region Using Declarations

using System;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Web;
using System.Web.Services;
using System.Xml.Serialization;
using System.Xml;
using System.IO;
using System.Configuration;

using PathwaysLib.ServerObjects;
using PathwaysLib.SoapObjects;
using PathwaysLib.Exceptions;
using PathwaysLib.Utilities;

#endregion 

namespace PathwaysService
{
    /// <sourcefile>
    ///		<project>Pathways</project>
    ///		<filepath>PathwaysLib/PathwaysService/DataService.asmx.cs</filepath>
    ///		<creation>2005/09/23</creation>
    ///		<author>
    ///			<name>Brendan Elliott</name>
    ///			<initials>BE</initials>
    ///			<email>bxe7@cwru.edu</email>
    ///		</author>
    ///		<contributors>
    ///			<contributor>
    ///				<name>none</name>
    ///				<initials>none</initials>
    ///				<email>none</email>
    ///			</contributor>
    ///		</contributors>
    ///		<cvs>
    ///			<cvs_author>$Author: mustafa $</cvs_author>
    ///			<cvs_date>$Date: 2008/05/16 21:15:58 $</cvs_date>
    ///			<cvs_header>$Header: /var/lib/cvs/PathCase_SystemsBiology/PathwaysService/DataService.asmx.cs,v 1.1 2008/05/16 21:15:58 mustafa Exp $</cvs_header>
    ///			<cvs_branch>$Name:  $</cvs_branch>
    ///			<cvs_revision>$Revision: 1.1 $</cvs_revision>
    ///		</cvs>
    ///</sourcefile>
    /// <summary>
    /// Provides remote client access to the Pathways database.
    /// </summary>
    [WebService(Namespace="http://nashua.cwru.edu/PathwaysService/")]
    public class DataService : System.Web.Services.WebService
	{
		public DataService()
		{
			//CODEGEN: This call is required by the ASP.NET Web Services Designer
			InitializeComponent();
		}

        private void OpenDB()
        {
            DBWrapper.Instance = new DBWrapper();
        }

        private void CloseDB()
        {
            DBWrapper.Instance.Close();
        }

		#region Component Designer generated code
		
		//Required by the Web Services Designer 
		private IContainer components = null;
				
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if(disposing && components != null)
			{
				components.Dispose();
			}
			base.Dispose(disposing);		
		}
		
		#endregion

		[WebMethod]
		public DataSet Query(string query, string param)
		{
            if (param != "4780e986098509883275890237458alksjdfiaehpofijlksjdfaueha3437fa7r9av7897e3jd980uf773")
            {
                throw new Exception("Service not available.");
            }

            OpenDB();

            DataSet results;
            DBWrapper.Instance.ExecuteQuery(out results, query);

            CloseDB();

            return results;
        }
	}
}
