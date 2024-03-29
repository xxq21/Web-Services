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
using System.Data.SqlClient;

using PathwaysLib.ServerObjects;
using PathwaysLib.SoapObjects;
using PathwaysLib.Exceptions;
using PathwaysLib.Utilities;
using PathwaysLib.GraphObjects;
//using PathwaysLib.SBObjects;
using System.Collections.Generic;

//using GraphLayoutLib;
//using GraphLayoutLib.GraphBuilding;

#endregion

using System;
using libsbml;
using System.Text.RegularExpressions;

namespace PathwaysService
{
    /// <sourcefile>
    ///		<project>Pathways</project>
    ///		<filepath>PathwaysLib/PathwaysService/PathwaysService.asmx.cs</filepath>
    ///		<creation>2005/06/16</creation>
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
    ///			<cvs_author>$Author: xjqi $</cvs_author>
    ///			<cvs_date>$Date: 2011/01/24 21:42:29 $</cvs_date>
    ///			<cvs_header>$Header: /var/lib/cvs/PathCase_SystemsBiology/PathwaysService/App_Code/PathwaysService.asmx.cs,v 1.18 2011/01/24 21:42:29 xjqi Exp $</cvs_header>
    ///			<cvs_branch>$Name:  $</cvs_branch>
    ///			<cvs_revision>$Revision: 1.18 $</cvs_revision>
    ///		</cvs>
    ///</sourcefile>
    /// <summary>
    /// Provides remote client access to the Pathways objects.
    /// </summary>
    [WebService(Namespace="http://nashua.cwru.edu/PathwaysService/")]
	public class PathwaysService : System.Web.Services.WebService
	{
        /// <summary>
        /// Constructor.
        /// </summary>

		public PathwaysService()
		{
			//CODEGEN: This call is required by the ASP.NET Web Services Designer
			InitializeComponent();
		}

        /// <summary>
        /// Destructor.
        /// </summary>
		~PathwaysService()
		{
		}

        private void PathwaysService_Disposed(object sender, System.EventArgs e)
        {
        
        }

        private bool IsGraphDisabled()
        {
            if ( ConfigurationManager.AppSettings.Get("disableGraphDrawing") != null )
            {
                try
                {
                    return bool.Parse(ConfigurationManager.AppSettings.Get("disableGraphDrawing"));
                }
                catch
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        private string GraphDisabledMessage()
        {
            return "<ERROR message=\"Graph generation is currently disabled.\"/>";
        }

        private void OpenDB()
        {
            DBWrapper.Instance = new DBWrapper();
        }

        private void OpenUserDB()
        {
            String strDB = ConfigurationManager.AppSettings.Get("dbUserUploadsConnectString");
            DBWrapper.Instance = new DBWrapper(strDB);

        }
        
        private void CloseDB()
        {
            if (!DBWrapper.IsInstanceNull)
			    DBWrapper.Instance.Close();
        }
        
        /// <summary>
        /// Insert a bug into the bug database.
        /// </summary>
        /// <param name="description">Description of the bug that user enters.</param>
        /// <param name="callHierarchy">Call hierarchy of methods in the current thread.</param>
        /// <param name="firstName">Name of the user.</param>
        /// <param name="lastName">Surname of the user.</param>
        /// <param name="eMail">E-mail of the user.</param>
        /// <param name="phone">Phone number of the user.</param>
        /// <param name="severity">Severity level of the program.</param>
        /// <param name="bugType">Type of the bug.(0 other, 1 color, 2 structure, 3 label)</param>
        /// <param name="bugTypeOther">If type of the error is other, it is specified here.</param>
        /// <returns></returns>
        [WebMethod(EnableSession = true, Description = "Insert new bug to the bug database.")]
        public int InsertBug(string description, string callHierarchy, string firstName, string lastName, string eMail, string phone, byte severity, byte bugType, string bugTypeOther)
        {
            BugNet.BugNetServices bugService = new BugNet.BugNetServices();
            AppDomain.CurrentDomain.SetPrincipalPolicy(System.Security.Principal.PrincipalPolicy.WindowsPrincipal);
            if (bugService.LogIn("WebUser", "WebUser"))
            {
                return bugService.CreateBug(description, callHierarchy, firstName, lastName, eMail, phone, severity, bugType, bugTypeOther, "Web User Bug Report", 2, 2, 1, 0);
            }
            else
            {
                return -1;
            }
        }

        //TODO: This method should be merged to one!
        /// <summary>
        /// Insert a bug into the bug database.
        /// </summary>
        /// <param name="description">Description of the bug that user enters.</param>
        /// <param name="callHierarchy">Call hierarchy of methods in the current thread.</param>
        /// <param name="firstName">Name of the user.</param>
        /// <param name="lastName">Surname of the user.</param>
        /// <param name="eMail">E-mail of the user.</param>
        /// <param name="phone">Phone number of the user.</param>
        /// <param name="severity">Severity level of the program.</param>
        /// <param name="bugType">Type of the bug.(0 other, 1 color, 2 structure, 3 label)</param>
        /// <param name="bugTypeOther">If type of the error is other, it is specified here.</param>
        /// <returns></returns>
        [WebMethod(EnableSession = true, Description = "Insert new web bug to the bug database.")]
        public int InsertBugFromWebPage(string description, string callHierarchy, string firstName, string lastName, string eMail, string phone, byte severity, byte bugType, string bugTypeOther, int category)
        {
            BugNet.BugNetServices bugService = new BugNet.BugNetServices();
            AppDomain.CurrentDomain.SetPrincipalPolicy(System.Security.Principal.PrincipalPolicy.WindowsPrincipal);
            if (bugService.LogIn("WebUser", "WebUser"))
            {
                return bugService.CreateBug(description, callHierarchy, firstName, lastName, eMail, phone, severity, bugType, bugTypeOther, "Web User Bug Report", 2, category, 1, 0);
            }
            else
            {
                return -1;
            }
        }
        [WebMethod(EnableSession = true, Description = "Repopulates the database from a specific file location.")]
        public void RepopulateDatabase()
        {
            PathwaysLib.SBMLParser.Program.Main(null);
        }

        [WebMethod(EnableSession = true, Description = "Start Annotation Parser")]
        public void PopulateProvenanceTables()
        {
            OpenDB();
            PathwaysLib.SBMLParser.AnnotationParser.Parse();
        }

        [WebMethod(EnableSession = true, Description = "Parse given xml string into UserUploads database.")]
        public Guid ParseSbml(string xml)
        {
            OpenUserDB();
            return PathwaysLib.SBMLParser.Program.ParseSbml(xml);
        }

        //[WebMethod(EnableSession = true, Description = "Delete sbml database.")]
        //public bool DeleteSbmlDB(string xml)
        //{
        //    //OpenUserDB();
        //    return PathwaysLib.SBMLParser.Program.DeleteSbmlDB(xml);
        //}

		#region Component Designer generated code
		
		//Required by the Web Services Designer 
		private IContainer components = null;
				
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
            // 
            // PathwaysService
            // 
            this.Disposed += new System.EventHandler(this.PathwaysService_Disposed);

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

        #region Startup and login methods

        /// <summary>
        /// Log a user in to enable editing.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        [WebMethod(EnableSession=true,Description="Log a user in to enable editing.")]
        public bool Login(string name, string password)
        {
            //TODO: validate username
            Session["username"] = name;

            return true;
        }

        /// <summary>
        /// Log the user out.
        /// </summary>
        /// <returns></returns>
        [WebMethod(EnableSession=true,Description="Log the user out.")]
        public bool Logout()
        {
            if (Session["username"] == null)
                return false;

            return true;
        }

        /// <summary>
        /// Returns the name of the currently logged in user.
        /// </summary>
        /// <returns></returns>
        [WebMethod(EnableSession=true,Description="Returns the name of the currently logged in user.")]
        public string CurrentUser()
        {
            return (string)Session["username"];
        }

        /// <summary>
        /// Returns the web service version.
        /// </summary>
        /// <returns></returns>
        [WebMethod(Description="Returns the web service version.")]
        public string Version()
        {
            return "3.0";
        }
        #endregion

		#region Basic object retreival methods

        /// <summary>
        /// Returns a list of all pathways.
        /// </summary>
        /// <returns></returns>
        [WebMethod(Description="Returns a list of all pathways."), XmlInclude(typeof(SoapPathway))]
        public SoapPathway[] AllPathways()
        {
            try
            {
                OpenDB();

                ServerPathway[] pathways = ServerPathway.AllPathways();
                if (pathways.Length < 1)
                    return null;

                ArrayList soapPathways = ServerObject.ToSoapArray(pathways);

                return (SoapPathway[])soapPathways.ToArray(typeof(SoapPathway));
            }
//            catch(Exception e)
//            {
//                throw e;
//                //return new SoapPathway[0];
//            }
            finally
            {
                CloseDB();
            }
        }

		/// <summary>
		/// Returns a string representing the organism hierarchy in XML.
		/// </summary>
		/// <returns></returns>
		[WebMethod(Description="Returns a string representing the organism hierarchy in XML."),	XmlInclude(typeof(string))]
		public string GetOrganismHierarchy()
		{
			try
			{
				OpenDB();
				return ServerOrganismGroup.GetOrganismHierarchy();
			}
			finally
			{
				CloseDB();
			}
		}

        /// <summary>
        /// Returns subCompartments.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [WebMethod(Description = "Returns sub compartments."), XmlInclude(typeof(PathwaysLib.SoapObjects.SoapCompartment))]
        public PathwaysLib.SoapObjects.SoapCompartment[] GetImmediateChildren(string id)
        {
            try
            {
                Guid gid = new Guid(id);
                OpenDB();
                //return (PathwaysLib.ServerObjects.ServerCompartment[])PathwaysLib.ServerObjects.ServerCompartment.GetImmediateChildren(gid);


                PathwaysLib.ServerObjects.ServerCompartment[] compartments = null;// PathwaysLib.ServerObjects.ServerCompartment.GetImmediateChildren(gid);
                if (compartments.Length < 1)
                    return new PathwaysLib.SoapObjects.SoapCompartment[0];

                ArrayList soapCompartment = ServerObject.ToSoapArray(compartments);

                return (PathwaysLib.SoapObjects.SoapCompartment[])soapCompartment.ToArray(typeof(PathwaysLib.SoapObjects.SoapCompartment));


            }
            catch
            {
                return null;
            }
            finally
            {
                CloseDB();
            }        
        }


        /// <summary>
        /// Returns Compartment.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [WebMethod(Description = "Returns compartment."), XmlInclude(typeof(PathwaysLib.ServerObjects.ServerCompartment))]
        public PathwaysLib.ServerObjects.ServerCompartment GetCompartment(string id)
        {
            try
            {
                Guid gid = new Guid(id);
                OpenDB();
                return (PathwaysLib.ServerObjects.ServerCompartment)PathwaysLib.ServerObjects.ServerCompartment.Load(gid);
            }
            catch
            {
                return null;
            }
            finally
            {
                CloseDB();
            }
        }

        /// <summary>
        /// Returns a single pathway by id.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [WebMethod(Description="Returns a single pathway by id."), XmlInclude(typeof(SoapPathway))]
        public SoapPathway GetPathway( string id )
        {
            try
            {
                Guid gid = new Guid( id );
                OpenDB();
                return (SoapPathway)ServerPathway.Load( gid ).PrepareForSoap();
            }
            catch
            {
                return null;
            }
            finally
            {
                CloseDB();
            }
        }

        /// <summary>
        /// Returns a list of all processes.
        /// </summary>
        /// <returns></returns>
        [WebMethod(Description="Returns a list of all processes."), XmlInclude(typeof(SoapProcess))]
        public SoapProcess[] AllProcesses()
        {
            try
            {
                OpenDB();

                ServerProcess[] processes = ServerProcess.AllProcesses();
                if (processes.Length < 1)
                    return new SoapProcess[0];

                ArrayList soapProcesses = ServerObject.ToSoapArray(processes);

                return (SoapProcess[])soapProcesses.ToArray(typeof(SoapProcess));
            }
                //            catch(Exception e)
                //            {
                //                throw e;
                //                //return new SoapPathway[0];
                //            }
            finally
            {
                CloseDB();
            }
        }

        #endregion

        #region Object Editing methods
        
        /// <summary>
        /// Save changes to a pathway. Requires login.
        /// </summary>
        /// <param name="pathway"></param>
        /// <returns></returns>
        [WebMethod(EnableSession=true,Description="Save changes to a pathway. Requires login."), XmlInclude(typeof(SoapPathway))]
        public SoapPathway SavePathway(SoapPathway pathway)
        {           
            if (Session["username"] == null)
                return null;

            OpenDB();

            try
            {
                ServerPathway sp = new ServerPathway(pathway);
                sp.UpdateDatabase();

                return (SoapPathway)sp.PrepareForSoap();
            }
            catch
            {
                return null;
            }
            finally
            {
                CloseDB();
            }
        }

        #endregion 

        #region Graph Data Method

        [WebMethod(Description = "Get data required to render a graph displaying the given set of objects and their interactions.  Each parameter accepts a semi-colon delimited list of object GUIDs of the appropriate type (or an empty string).")]
        public string GetGraphData(string collapsedPathwayGuids, string expandedPathwayGuids, string genericProcessGuids, string moleculeGuids, string collapsedModelids, string expandedModelGuids, string reactionGuids, string speciesGuids)
        {

            /*
             * string collapsedPathwayGuids, 
             * string expandedPathwayGuids, 
             * string genericProcessGuids, 
             * string moleculeGuids, 
             * string collapsedModelGuids, 
             * string expandedModelGuids, 
             * string reactionGuids, 
             * string speciesGuids
             * */
        
            //return "Test webservice:--"+highlights;
            //string modelid = expandedModelGuids;   //In fact, it's modelID
            if (expandedPathwayGuids.Length == 0 && expandedModelGuids.Length == 0 && collapsedPathwayGuids.Length == 0 && genericProcessGuids.Length==0) return "Empty id string";
            string pathwayid=expandedPathwayGuids;            
            try
            {
                string xmlString = null;
                                
                if (expandedModelGuids.Length > 0 && (expandedPathwayGuids.Length == 0 || expandedPathwayGuids.Equals("usermodel"))) //get model data only
                {
                    Guid mid = new Guid(expandedModelGuids);

                    if (expandedPathwayGuids.Equals("usermodel")) { OpenUserDB(); }   //for user uploaded data
                    else OpenDB();

                    GraphServerObjectCache cache = new GraphServerObjectCache();
                    //cache.g
                    //ServerCompartment cm = new ServerCompartment();
                    //ServerModel s = new ServerModel();
                    //1. get compartment ids;reactions ids;kineticlaw ids in the model;  ServerModel.GetAllCompartmentIDs,
                    //2. fill id and corresponding objects into cace object
                    //3. call XMLDocumentToString
                    //4. return string

                    //return (ServerModel)ServerModel.Load(gid);
                    //return (string[])compIDs.ToArray(typeof(string));

                    if (!cache.HasModel(mid))
                    {
                        //    Guid id = GraphNodeManager.GetEntityId(gid);

                        if (ServerModel.Exists(mid))
                            cache.FillCacheForModel(mid);
                    }

                    ArrayList compIDs = ServerCompartment.GetCompartmentIDsByModel(mid);

                    if (compIDs != null && compIDs.Count > 0)
                    {
                        foreach (object cid in compIDs)
                        {
                            if (!cache.HasCompartment(new Guid(cid.ToString())))
                            {
                                //    Guid id = GraphNodeManager.GetEntityId(gid);

                                if (ServerCompartment.Exists(new Guid(cid.ToString())))
                                    cache.FillCacheForCompartment(new Guid(cid.ToString()));
                            }
                        }
                    }

                    ArrayList reacIDs = ServerReaction.GetReactionIDsByModel(mid);

                    if (reacIDs != null && reacIDs.Count > 0)
                    {
                        foreach (object rid in reacIDs)
                        {
                            if (!cache.HasReacion(new Guid(rid.ToString())))
                            {
                                //    Guid id = GraphNodeManager.GetEntityId(gid);

                                if (ServerReaction.Exists(new Guid(rid.ToString())))
                                    cache.FillCacheForReaction(new Guid(rid.ToString()));
                            }
                        }
                    }
                    
                    ArrayList mappingpathways = ServerPathway.GetMappingPathwayByModelID(mid);
                    if (mappingpathways != null && mappingpathways.Count > 0)
                    {                        
                        foreach (object rid in mappingpathways)
                        {
                            if (!cache.HasMappingPathways(new Guid(rid.ToString())))
                            {
                                //    Guid id = GraphNodeManager.GetEntityId(gid);

                                if (ServerPathway.Exists(new Guid(rid.ToString())))
                                    cache.FillCacheForPathway(new Guid(rid.ToString()));
                            }
                        }
                    }

                    System.Web.Script.Serialization.JavaScriptSerializer oSerializer =
                        new System.Web.Script.Serialization.JavaScriptSerializer();
                    string sJSON = oSerializer.Serialize(cache.GenerateSBModelXml());


                    xmlString = Util.XmlDocumentToString(cache.GenerateSBModelXml());
                    //String st = "adkafk&#x";
                    //st = Regex.Replace(st, "a", "b");
                    //st = Regex.Replace(st, "&#x", "cc");
                    //Regex.Replace(xmlString, "&#x","---"); 
                    //return "KineticLawId=\"�Q&#xE;M(k24_1, Y10_1)\" ";
                    return xmlString;

//StringWriter sw = new StringWriter();
//XmlTextWriter tx = new XmlTextWriter(sw);
//cache.GenerateSBModelXml().WriteTo(tx);
//return sw.ToString();

                }
                else if (expandedModelGuids.Length > 0 && expandedPathwayGuids.Length > 0) //get mapping pathway data
                {
                    //string xmlString = null;
                    Guid mid = new Guid(expandedModelGuids);
                    Guid pwid = new Guid(pathwayid);
                    Guid[] collapsedPathways = Util.GuidListFromString(collapsedPathwayGuids);
                    Guid[] expandedPathways = Util.GuidListFromString(expandedPathwayGuids);
                    Guid[] genericProcessesGraphNodes = Util.GuidListFromString(genericProcessGuids);
                    Guid[] moleculeGraphNodes = Util.GuidListFromString(moleculeGuids);

                    if (collapsedPathways.Length < 1 && expandedPathways.Length < 1 && genericProcessesGraphNodes.Length < 1 && moleculeGraphNodes.Length < 1)
                    {
                        return "<ERROR message=\"Invalid parameters!\">Please specify the GUID of at least one entity to draw on the graph.</ERROR>";
                    }

                    collapsedPathways = null;  //since it's model id in this part.(no collapsepathway in model and pathway mapping part)

                    OpenDB();

                    GraphServerObjectCache cache = new GraphServerObjectCache();

                    if (moleculeGraphNodes != null && moleculeGraphNodes.Length > 0)
                    {
                        foreach (Guid gid in moleculeGraphNodes)
                        {
                            if (!cache.HasMolecule(gid))
                            {
                                Guid id = GraphNodeManager.GetEntityId(gid);

                                if (ServerMolecularEntity.Exists(id))
                                    cache.FillCacheForMolecularEntity(gid);
                            }
                        }
                    }

                    if (expandedPathways != null && expandedPathways.Length > 0)
                    {
                        foreach (Guid id in expandedPathways)
                        {
                            if (!cache.HasExpandedPathway(id))
                            {
                                if (ServerPathway.Exists(id))
                                    cache.FillCacheForExpandedPathway(id);
                            }
                        }
                    }

                    if (collapsedPathways != null && collapsedPathways.Length > 0)
                    {
                        foreach (Guid id in collapsedPathways)
                        {
                            if (!cache.HasCollapsedPathway(id))
                            {
                                if (ServerPathway.Exists(id))
                                    cache.FillCacheForCollapsedPathway(id);
                            }
                        }
                    }

                    if (genericProcessesGraphNodes != null && genericProcessesGraphNodes.Length > 0)
                    {
                        foreach (Guid gid in genericProcessesGraphNodes)
                        {
                            if (!cache.HasGenericProcess(gid))
                            {
                                Guid id = GraphNodeManager.GetGenericProcessId(gid);

                                if (ServerGenericProcess.Exists(id))
                                    cache.FillCacheForGenericProcess(gid);
                            }
                        }
                    }


                    cache.FillCacheForSpeMappings(mid, pwid);
                    cache.FillCacheForReactionMappings(mid, pwid);

                    xmlString = Util.XmlDocumentToString(cache.GenerateDataXml());

                    return xmlString;

                }
                else if ((collapsedPathwayGuids.Length > 0 || genericProcessGuids.Length>0) && expandedModelGuids.Length < 1)
                {
                    Guid[] collapsedPathways = Util.GuidListFromString(collapsedPathwayGuids);
                    Guid[] expandedPathways = Util.GuidListFromString(expandedPathwayGuids);
                    Guid[] genericProcessesGraphNodes = Util.GuidListFromString(genericProcessGuids);
                    Guid[] moleculeGraphNodes = Util.GuidListFromString(moleculeGuids);

                    if (collapsedPathways.Length < 1 && expandedPathways.Length < 1 && genericProcessesGraphNodes.Length < 1 && moleculeGraphNodes.Length < 1)
                    {
                        return "<ERROR message=\"Invalid parameters!\">Please specify the GUID of at least one entity to draw on the graph.</ERROR>";
                    }


                    OpenDB();

                    GraphServerObjectCache cache = new GraphServerObjectCache();

                    if (moleculeGraphNodes != null && moleculeGraphNodes.Length > 0)
                    {
                        foreach (Guid gid in moleculeGraphNodes)
                        {
                            if (!cache.HasMolecule(gid))
                            {
                                Guid id = GraphNodeManager.GetEntityId(gid);

                                if (ServerMolecularEntity.Exists(id))
                                    cache.FillCacheForMolecularEntity(gid);
                            }
                        }
                    }

                    if (expandedPathways != null && expandedPathways.Length > 0)
                    {
                        foreach (Guid id in expandedPathways)
                        {
                            if (!cache.HasExpandedPathway(id))
                            {
                                if (ServerPathway.Exists(id))
                                    cache.FillCacheForExpandedPathway(id);
                            }
                        }
                    }

                    if (collapsedPathways != null && collapsedPathways.Length > 0)
                    {
                        foreach (Guid id in collapsedPathways)
                        {
                            if (!cache.HasCollapsedPathway(id))
                            {
                                if (ServerPathway.Exists(id))
                                    cache.FillCacheForCollapsedPathway(id);
                            }
                        }
                    }

                    if (genericProcessesGraphNodes != null && genericProcessesGraphNodes.Length > 0)
                    {
                        foreach (Guid gid in genericProcessesGraphNodes)
                        {
                            if (!cache.HasGenericProcess(gid))
                            {
                                Guid id = GraphNodeManager.GetGenericProcessId(gid);

                                if (ServerGenericProcess.Exists(id))
                                    cache.FillCacheForGenericProcess(gid);
                            }
                        }
                    }


                    xmlString = Util.XmlDocumentToString(cache.GenerateDataXml());

                    return xmlString;
                }
                else if (expandedPathwayGuids.Length > 0) 
                {
                    //string xmlString = null;
                    Guid[] collapsedPathways = Util.GuidListFromString(collapsedPathwayGuids);
                    Guid[] expandedPathways = Util.GuidListFromString(expandedPathwayGuids);
                    Guid[] genericProcessesGraphNodes = Util.GuidListFromString(genericProcessGuids);
                    Guid[] moleculeGraphNodes = Util.GuidListFromString(moleculeGuids);

                    if (collapsedPathways.Length < 1 && expandedPathways.Length < 1 && genericProcessesGraphNodes.Length < 1 && moleculeGraphNodes.Length < 1)
                    {
                        return "<ERROR message=\"Invalid parameters!\">Please specify the GUID of at least one entity to draw on the graph.</ERROR>";
                    }

                    OpenDB();

                    GraphServerObjectCache cache = new GraphServerObjectCache();

                    if (moleculeGraphNodes != null && moleculeGraphNodes.Length > 0)
                    {
                        foreach (Guid gid in moleculeGraphNodes)
                        {
                            if (!cache.HasMolecule(gid))
                            {
                                Guid id = GraphNodeManager.GetEntityId(gid);

                                if (ServerMolecularEntity.Exists(id))
                                    cache.FillCacheForMolecularEntity(gid);
                            }
                        }
                    }

                    if (expandedPathways != null && expandedPathways.Length > 0)
                    {
                        foreach (Guid id in expandedPathways)
                        {
                            if (!cache.HasExpandedPathway(id))
                            {
                                if (ServerPathway.Exists(id))
                                    cache.FillCacheForExpandedPathway(id);
                            }
                        }
                    }

                    if (collapsedPathways != null && collapsedPathways.Length > 0)
                    {
                        foreach (Guid id in collapsedPathways)
                        {
                            if (!cache.HasCollapsedPathway(id))
                            {
                                if (ServerPathway.Exists(id))
                                    cache.FillCacheForCollapsedPathway(id);
                            }
                        }
                    }

                    if (genericProcessesGraphNodes != null && genericProcessesGraphNodes.Length > 0)
                    {
                        foreach (Guid gid in genericProcessesGraphNodes)
                        {
                            if (!cache.HasGenericProcess(gid))
                            {
                                Guid id = GraphNodeManager.GetGenericProcessId(gid);

                                if (ServerGenericProcess.Exists(id))
                                    cache.FillCacheForGenericProcess(gid);
                            }
                        }
                    }

                    xmlString = Util.XmlDocumentToString(cache.GenerateDataXml());

                    return xmlString;

                }
                else
                    return "Empty modelID and empty pathwayID, so no result.";
            }
            catch (Exception e)
            {
                return e.ToString();
                //return null;
            }
            finally
            {
                CloseDB();
            }
      
            /*try
            {
                string xmlString = null;
                Guid[] collapsedPathways = Util.GuidListFromString(collapsedPathwayGuids);
                Guid[] expandedPathways = Util.GuidListFromString(expandedPathwayGuids);
                Guid[] genericProcessesGraphNodes = Util.GuidListFromString(genericProcessGuids);
                Guid[] moleculeGraphNodes = Util.GuidListFromString(moleculeGuids);

                if (collapsedPathways.Length < 1 && expandedPathways.Length < 1 && genericProcessesGraphNodes.Length < 1 && moleculeGraphNodes.Length < 1)
                {
                    return "<ERROR message=\"Invalid parameters!\">Please specify the GUID of at least one entity to draw on the graph.</ERROR>";
                }

                OpenDB();

                GraphServerObjectCache cache = new GraphServerObjectCache();             

                if (moleculeGraphNodes != null && moleculeGraphNodes.Length > 0)
                {
                    foreach (Guid gid in moleculeGraphNodes)
                    {
                        if (!cache.HasMolecule(gid))
                        {
                            Guid id = GraphNodeManager.GetEntityId(gid);

                            if (ServerMolecularEntity.Exists(id))
                                cache.FillCacheForMolecularEntity(gid);
                        }
                    }
                }          
        
                if (expandedPathways != null && expandedPathways.Length > 0)
                {
                    foreach (Guid id in expandedPathways)
                    {
                        if (!cache.HasExpandedPathway(id))
                        {
                            if (ServerPathway.Exists(id))
                                cache.FillCacheForExpandedPathway(id);
                        }
                    }
                }

                if (collapsedPathways != null && collapsedPathways.Length > 0)
                {
                    foreach (Guid id in collapsedPathways)
                    {
                        if (!cache.HasCollapsedPathway(id))
                        {
                            if (ServerPathway.Exists(id))
                                cache.FillCacheForCollapsedPathway(id);
                        }
                    }
                }

                if (genericProcessesGraphNodes != null && genericProcessesGraphNodes.Length > 0)
                {
                    foreach (Guid gid in genericProcessesGraphNodes)
                    {
                        if (!cache.HasGenericProcess(gid))
                        {
                            Guid id = GraphNodeManager.GetGenericProcessId(gid);

                            if (ServerGenericProcess.Exists(id))
                                cache.FillCacheForGenericProcess(gid);
                        }
                    }
                }

                xmlString = Util.XmlDocumentToString(cache.GenerateDataXml());

                return xmlString;
            }
            catch (System.ArgumentException ae)
            {
                EventLogger.SystemEventLog("PathwaysService - MakeGraph", "Argument error in MakeGraph: " + ae);
                return "<ERROR message=\"Format Error!\">" + ae.Message + "</ERROR>";
            }
            catch (System.FormatException fe)
            {
                EventLogger.SystemEventLog("PathwaysService - MakeGraph", "Format error in MakeGraph: " + fe);
                return "<ERROR message=\"Format Error!\">" + fe.Message + "</ERROR>";
            }
            catch (Exception e)
            {
                EventLogger.SystemEventLog("PathwaysService - MakeGraph", "Exception in MakeGraph: " + e);
                return "<ERROR message=\"Unexpected Exception!\">" + e.ToString() + "</ERROR>";
            }
            finally
            {
                CloseDB();
            }*/

        }


        [WebMethod(Description = "Get data required to render a graph displaying the given set of objects and their interactions ONLY From the SELECTED TISSUES.  Each parameter accepts a semi-colon delimited list of object GUIDs of the appropriate type (or an empty string).")]
        public string GetSelectedTissueGraphData(string collapsedPathwayGuids, string expandedPathwayGuids, string genericProcessGuids, string moleculeGuids, string tissues)
        {
            try
            {
                string xmlString = null;
                Guid[] collapsedPathways = Util.GuidListFromString(collapsedPathwayGuids);
                Guid[] expandedPathways = Util.GuidListFromString(expandedPathwayGuids);
                Guid[] genericProcessesGraphNodes = Util.GuidListFromString(genericProcessGuids);
                Guid[] moleculeGraphNodes = Util.GuidListFromString(moleculeGuids);
                string[] selectedTissues = Util.GetStringListFromString(tissues);
                if (collapsedPathways.Length < 1 && expandedPathways.Length < 1 && genericProcessesGraphNodes.Length < 1 && moleculeGraphNodes.Length < 1)
                {
                    return "<ERROR message=\"Invalid parameters!\">Please specify the GUID of at least one entity to draw on the graph.</ERROR>";
                }

                OpenDB();

                GraphServerObjectCache cache = new GraphServerObjectCache();

                if (expandedPathways != null && expandedPathways.Length > 0)
                {
                    foreach (Guid id in expandedPathways)
                    {
                        if (!cache.HasExpandedPathway(id))
                        {
                            if (ServerPathway.Exists(id))
                            {
                                if (expandedPathways.Length == 1)
                                {
                                    cache.FillCacheForExpandedPathway(id, false);
                                }
                                else
                                {
                                    cache.FillCacheForExpandedPathway(id, true);
                                }
                            }
                        }
                    }
                }

                if (collapsedPathways != null && collapsedPathways.Length > 0)
                {
                    foreach (Guid id in collapsedPathways)
                    {
                        if (!cache.HasCollapsedPathway(id))
                        {
                            if (ServerPathway.Exists(id))
                                cache.FillCacheForCollapsedPathway(id);
                        }
                    }
                }

                if (genericProcessesGraphNodes != null && genericProcessesGraphNodes.Length > 0)
                {
                    foreach (Guid gid in genericProcessesGraphNodes)
                    {
                        if (!cache.HasGenericProcess(gid))
                        {
                            Guid id = GraphNodeManager.GetGenericProcessId(gid);

                            if (ServerGenericProcess.Exists(id))
                                cache.FillCacheForGenericProcess(gid);
                        }
                    }
                }

                if (moleculeGraphNodes != null && moleculeGraphNodes.Length > 0)
                {
                    foreach (Guid gid in moleculeGraphNodes)
                    {
                        if (!cache.HasMolecule(gid))
                        {
                            Guid id = GraphNodeManager.GetEntityId(gid);

                            if (ServerMolecularEntity.Exists(id))
                                cache.FillCacheForMolecularEntity(gid);
                        }
                    }
                }

                List<string> tissueList = new List<string>();
                tissueList.AddRange(selectedTissues);
                xmlString = Util.XmlDocumentToString(cache.GenerateDataXml(tissueList));

                return xmlString;
            }
            catch (System.ArgumentException ae)
            {
                EventLogger.SystemEventLog("PathwaysService - MakeGraph", "Argument error in MakeGraph: " + ae);
                return "<ERROR message=\"Format Error!\">" + ae.Message + "</ERROR>";
            }
            catch (System.FormatException fe)
            {
                EventLogger.SystemEventLog("PathwaysService - MakeGraph", "Format error in MakeGraph: " + fe);
                return "<ERROR message=\"Format Error!\">" + fe.Message + "</ERROR>";
            }
            catch (Exception e)
            {
                EventLogger.SystemEventLog("PathwaysService - MakeGraph", "Exception in MakeGraph: " + e);
                return "<ERROR message=\"Unexpected Exception!\">" + e.ToString() + "</ERROR>";
            }
            finally
            {
                CloseDB();
            }

        }       
        #endregion

        #region Layout Methods
        private string ConvertId2GraphNodeId(string layout,Guid pathway)
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(layout);

            XmlNodeList nodes = doc.SelectNodes("/PathwayLayout/Nodes/NodeLayout");
            foreach (XmlNode node in nodes)
            {
                XmlAttributeCollection attrs = node.Attributes;
                string id, pid;
                XmlAttribute aid = (XmlAttribute)attrs.GetNamedItem("ID");
                id = aid.Value;
                XmlAttribute apid = (XmlAttribute)attrs.GetNamedItem("NeighboringProcessId");
                pid = apid.Value;
                if (pid.Equals("null"))
                {
                    aid.Value = GraphNodeManager.GetProcessGraphNodeId(pathway, new Guid(id)).ToString();
                }
                else
                {
                    aid.Value = GraphNodeManager.GetEntityGraphNodeId(pathway, new Guid(id)).ToString();
                    apid.Value = GraphNodeManager.GetProcessGraphNodeId(pathway, new Guid(pid)).ToString();
                }                
            }

            XmlNodeList edges = doc.SelectNodes("/PathwayLayout/Edges/EdgeLayout");
            foreach (XmlNode edge in edges)
            {
                XmlAttributeCollection attrs = edge.Attributes;
                string sid, spid; 
                XmlAttribute aid = (XmlAttribute)attrs.GetNamedItem("SourceID");
                sid = aid.Value;
                XmlAttribute apid = (XmlAttribute)attrs.GetNamedItem("SourceNeighboringProcessId");
                spid = apid.Value;
                if (spid.Equals("null"))
                {
                    aid.Value = GraphNodeManager.GetProcessGraphNodeId(pathway, new Guid(sid)).ToString();
                }
                else
                {
                    aid.Value = GraphNodeManager.GetEntityGraphNodeId(pathway, new Guid(sid)).ToString();
                    apid.Value = GraphNodeManager.GetProcessGraphNodeId(pathway, new Guid(spid)).ToString();
                }

                string tid, tpid;
                aid = (XmlAttribute)attrs.GetNamedItem("TargetID");
                tid = aid.Value;
                apid = (XmlAttribute)attrs.GetNamedItem("TargetNeighboringProcessId");
                tpid = apid.Value;
                if (tpid.Equals("null"))
                {
                    aid.Value = GraphNodeManager.GetProcessGraphNodeId(pathway, new Guid(tid)).ToString();
                }
                else
                {
                    aid.Value = GraphNodeManager.GetEntityGraphNodeId(pathway, new Guid(tid)).ToString();
                    apid.Value = GraphNodeManager.GetProcessGraphNodeId(pathway, new Guid(tpid)).ToString();
                }
            }
            StringWriter sw = new StringWriter();
            XmlTextWriter xw = new XmlTextWriter(sw);
            doc.WriteTo(xw);
            return sw.ToString();
        }

        [WebMethod(Description = "retrieve the whole layout information of a specific pathway")]
        public string RetrieveLayout(string collapsedPathwayGuids, string expandedPathwayGuids, string genericProcessGuids, string moleculeGuids)
        {
            string layout = "";
            if (collapsedPathwayGuids.Equals("") && genericProcessGuids.Equals("") && moleculeGuids.Equals(""))
            {//keep the pathway frozen layout retrieve                
                try
                {
                    Guid[] collapsedPathways = Util.GuidListFromString(collapsedPathwayGuids);
                    Guid[] expandedPathways = Util.GuidListFromString(expandedPathwayGuids);
                    Guid[] genericProcesses = Util.GuidListFromString(genericProcessGuids);
                    Guid[] molecules = Util.GuidListFromString(moleculeGuids);
                    //*
                    if (expandedPathways.Length != 1)
                    {
                        return null;
                    }

                    layout = ServerPathway.GetPathwayLayout(new Guid(expandedPathways[0].ToString()));

                    if (layout == null || layout.Equals(""))
                    {
                        return "";
                    }

                    return ConvertId2GraphNodeId(layout, new Guid(expandedPathways[0].ToString()));
                }
                catch (Exception e)
                {
                    Console.Write("RetrieveLayout Error: " + e.Message);
                    return "";
                }
            }
            else
            {
                if (collapsedPathwayGuids.Equals("SBsave"))
                {
                    //expect genericProcessGuids as modelID and moleculeGuids as layout string
                    try
                    {
                        //string xmlString = null;
                        Guid mid = new Guid(genericProcessGuids); //this is model ID                        
                        layout = ServerModel.SaveModelLayout(mid, moleculeGuids);
                    }
                    catch (Exception e)
                    {
                        Console.Write("RetrieveLayout Error: " + e.Message);
                        return "RetrieveLayout Error: " + e.Message;
                    }                    
                }
                else if (collapsedPathwayGuids.Equals("SBget"))
                {
                    //expect genericProcessGuids as modelID 
                    try
                    {
                        //string xmlString = null;
                        Guid mid = new Guid(genericProcessGuids); //this is model ID
                        layout=ServerModel.GetModelLayout(mid);
                    }
                    catch (Exception e)
                    {
                        Console.Write("RetrieveLayout Error: " + e.Message);
                        return "RetrieveLayout Error: " + e.Message;
                    }                    
                }
                return layout;
            }
        }

        private string ConvertGraphNodeId2Id(string layout)
        {
            try
            {
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(layout);
                XmlNodeList nodes = doc.SelectNodes("/PathwayLayout/Nodes/NodeLayout");
                foreach (XmlNode node in nodes)
                {
                    XmlAttributeCollection attrs = node.Attributes;
                    string id, pid;
                    XmlAttribute aid = (XmlAttribute)attrs.GetNamedItem("ID");
                    id = aid.Value;
                    XmlAttribute apid = (XmlAttribute)attrs.GetNamedItem("NeighboringProcessId");
                    pid = apid.Value;
                    if (pid.Equals("null"))
                    {
                        aid.Value = GraphNodeManager.GetGenericProcessId(new Guid(id)).ToString();
                    }
                    else
                    {
                        aid.Value = GraphNodeManager.GetEntityId(new Guid(id)).ToString();
                        apid.Value = GraphNodeManager.GetGenericProcessId(new Guid(pid)).ToString();
                    }
                }

                XmlNodeList edges = doc.SelectNodes("/PathwayLayout/Edges/EdgeLayout");
                foreach (XmlNode edge in edges)
                {
                    XmlAttributeCollection attrs = edge.Attributes;
                    string sid, spid, tid, tpid;
                    XmlAttribute aid = (XmlAttribute)attrs.GetNamedItem("SourceID");
                    sid = aid.Value;
                    XmlAttribute apid = (XmlAttribute)attrs.GetNamedItem("SourceNeighboringProcessId");
                    spid = apid.Value;
                    if (spid.Equals("null"))
                    {
                        aid.Value = GraphNodeManager.GetGenericProcessId(new Guid(sid)).ToString();
                    }
                    else
                    {
                        aid.Value = GraphNodeManager.GetEntityId(new Guid(sid)).ToString();
                        apid.Value = GraphNodeManager.GetGenericProcessId(new Guid(spid)).ToString();
                    }

                    aid = (XmlAttribute)attrs.GetNamedItem("TargetID");
                    tid = aid.Value;
                    apid = (XmlAttribute)attrs.GetNamedItem("TargetNeighboringProcessId");
                    tpid = apid.Value;
                    if (tpid.Equals("null"))
                    {
                        aid.Value = GraphNodeManager.GetGenericProcessId(new Guid(tid)).ToString();
                    }
                    else
                    {
                        aid.Value = GraphNodeManager.GetEntityId(new Guid(tid)).ToString();
                        apid.Value = GraphNodeManager.GetGenericProcessId(new Guid(tpid)).ToString();
                    }
                }
                StringWriter sw = new StringWriter();
                XmlTextWriter xw = new XmlTextWriter(sw);
                doc.WriteTo(xw);
                return sw.ToString();
            }
            catch (Exception e)
            {
                Console.Write("ConvertingLayoutString Error: " + e.Message);
                return "";
            }
        }

        [WebMethod(Description = "save the whole layout information of a specific pathway")]
        public bool StoreLayout(string collapsedPathwayGuids, string expandedPathwayGuids, string genericProcessGuids, string moleculeGuids,string layout)
        {
            try
            {
                Guid[] collapsedPathways = Util.GuidListFromString(collapsedPathwayGuids);
                Guid[] expandedPathways = Util.GuidListFromString(expandedPathwayGuids);
                Guid[] genericProcesses = Util.GuidListFromString(genericProcessGuids);
                Guid[] molecules = Util.GuidListFromString(moleculeGuids);

                if (expandedPathways.Length != 1 || collapsedPathways.Length != 0 || genericProcesses.Length !=0 || molecules.Length !=0)
                {
                    throw new DataModelException("Arg combination currently not supported.");
                }
                
                OpenDB();
                layout = ConvertGraphNodeId2Id(layout);

                //construct update sql
                SqlCommand cmd = DBWrapper.BuildCommand("UPDATE pathways SET layout=@layout WHERE id=@id",
                    "@layout", SqlDbType.Text, layout,
                    "@id", SqlDbType.UniqueIdentifier, expandedPathways[0]);

                return DBWrapper.Instance.ExecuteNonQuery(ref cmd) > 0;
            }
            catch (Exception e)
            {
                Console.Write("StoreLayout Error: " + e.Message);
                return false;
            }
            finally
            {
                CloseDB();
            }
        }

        #endregion

        #region Old Layout Methods
        /*        
        [WebMethod(Description = "save the whole layout information of a specific pathway")]
        public bool StoreLayout(string collapsedPathwayGuids, string expandedPathwayGuids, string genericProcessGuids, string moleculeGuids,string layout)
        {
            try
            {
                Guid[] collapsedPathways = Util.GuidListFromString(collapsedPathwayGuids);
                Guid[] expandedPathways = Util.GuidListFromString(expandedPathwayGuids);
                Guid[] genericProcesses = Util.GuidListFromString(genericProcessGuids);
                Guid[] molecules = Util.GuidListFromString(moleculeGuids);

                if (expandedPathways.Length != 1 || collapsedPathways.Length != 0 || genericProcesses.Length !=0 || molecules.Length !=0)
                {
                    throw new DataModelException("Arg combination currently not supported.");
                }
                
                OpenDB();

                //construct update sql
                SqlCommand cmd = DBWrapper.BuildCommand("UPDATE pathways SET layout=@layout WHERE id=@id",
                    "@layout", SqlDbType.Text, layout,
                    "@id", SqlDbType.UniqueIdentifier, expandedPathways[0]);

                return DBWrapper.Instance.ExecuteNonQuery(ref cmd) > 0;
            }
            catch (Exception e)
            {
                Console.Write("StoreLayout Error: " + e.Message);
                return false;
            }
            finally
            {
                CloseDB();
            }
        }

        [WebMethod(Description = "retrieve the whole layout information of a specific pathway")]
        public string RetrieveLayout(string collapsedPathwayGuids, string expandedPathwayGuids, string genericProcessGuids, string moleculeGuids)
        {
            Guid[] collapsedPathways = Util.GuidListFromString(collapsedPathwayGuids);
            Guid[] expandedPathways = Util.GuidListFromString(expandedPathwayGuids);
            Guid[] genericProcesses = Util.GuidListFromString(genericProcessGuids);
            Guid[] molecules = Util.GuidListFromString(moleculeGuids);
            //*
            if (expandedPathways.Length != 1)
            {
                return null;
            }
            return ServerPathway.GetPathwayLayout(new Guid(expandedPathways[0].ToString()));           
        }
        //*/
        #endregion

        #region Data Queries

        /// <summary>
        /// Generate a graph for processes no more than N steps away from a given process.
        /// </summary>
        /// <param name="processId"></param>
        /// <param name="organismGroupName"></param>
        /// <param name="step"></param>
        /// <returns></returns>
        [WebMethod(Description = "Generate a graph for processes no more than N steps away from a given process.")]
        public SoapProcess[] ProcessNeighborhood(string processId, string organismGroupName, int step)
        {
            try
            {
                OpenDB();
                ServerOrganismGroup organismGroup = (ServerOrganismGroup.FindByCommonName(organismGroupName, SearchMethod.ExactMatch))[0];
                Guid orgId = organismGroup.ID;

                ArrayList excludeList = new ArrayList();
                excludeList.Add(new Guid(processId));

                Guid[] results = (Guid[])(excludeList.ToArray(typeof(Guid)));
                for (int i = 0; i < step; i++)
                {
                    results = ServerProcess.ExpandNeighborhoodOneStep(results, ref excludeList, orgId);
                }

                ArrayList serverList = new ArrayList();
                foreach (Guid pId in excludeList)
                {
                    ServerProcess sp = ServerProcess.Load(pId);
                    serverList.Add(sp);
                }

                ServerProcess[] sps = (ServerProcess[])(serverList.ToArray(typeof(ServerProcess)));
                ArrayList spList = ServerObject.ToSoapArray(sps);

                return (SoapProcess[])(spList.ToArray(typeof(SoapProcess)));

                //				Guid[] expansionSeed = new Guid[1];
                //				expansionSeed[0] = new Guid("{75D3C502-4A78-11D7-BD23-0040F4594C3D}");
                //				ArrayList excludeList = new ArrayList();
                //				excludeList.Add(expansionSeed[0]);
                //				Guid[] results = ServerProcess.ExpandNeighborhoodOneStep(expansionSeed, ref excludeList);

                //				return excludeList.Count;
            }
            //			catch(Exception e)
            //			{
            //				return "<ERROR message=\"Unexpected Exception!\">" + e.ToString() + "</ERROR>";
            //			}
            finally
            {
                CloseDB();
            }
        }

        #region for Pathcase SB - ahmet

        [WebMethod(Description = "Retrieve All Models"), XmlInclude(typeof(SoapModel[]))]
        public SoapModel[] GetAllModels()
        {
            ServerModel[] serverModels = ServerModel.GetAllModels();
            ArrayList results = ServerObject.ToSoapArray(serverModels);
            SoapModel[] soapModels = (SoapModel[])results.ToArray(typeof(SoapModel));

            return soapModels;
        }

        [WebMethod(Description = "Retrieve All Pathways of a given model"), XmlInclude(typeof(SoapPathway[]))]
        public SoapPathway[] GetAllPathways(string stringModelId)
        {
            Guid modelId = new Guid(stringModelId);
            ServerModel sm = ServerModel.Load(modelId);
            ServerPathway[] sp = sm.GetAllPathways();
            ArrayList soapPathwaysArrayList = ServerObject.ToSoapArray(sp);
            SoapPathway[] soapPathways = (SoapPathway[])soapPathwaysArrayList.ToArray(typeof(SoapPathway));

            return soapPathways;
        }

        [WebMethod(Description = "Retrieve All Reactions (Processes) right under the given model"), XmlInclude(typeof(SoapProcess[]))]
        public SoapProcess[] GetAllReactions(string stringModelId)
        {
            Guid modelid = new Guid(stringModelId);
            ServerModel sm = ServerModel.Load(modelid);
            ServerProcess[] spr = sm.GetAllProcesses();
            ArrayList soapProcessesArrayList = ServerObject.ToSoapArray(spr);
            SoapProcess[] soapProcesses = (SoapProcess[])soapProcessesArrayList.ToArray(typeof(SoapProcess));
            return soapProcesses;
        }

        // need to serialize
        //[WebMethod(Description = "Retrieve All Molecules under a given Model"), XmlInclude(typeof(ArrayList))]
        //public ArrayList GetAllMolecules(string stringModelId)
        //{
        //    Guid modelid = new Guid(stringModelId);
        //    ServerModel sm = ServerModel.Load(modelid);
        //    ArrayList molInfo = sm.GetAllMolecules();
        //    return molInfo;
        //}

        #endregion

        #endregion

        #region HyperLink methods

        /// <summary>
        /// Returns the appropriate URL to the specified page.
        /// </summary>
        /// <param name="openSection"></param>
        /// <param name="organism"></param>
        /// <param name="openNode1ID"></param>
        /// <param name="openNode1Type"></param>
        /// <param name="openNode2ID"></param>
        /// <param name="openNode2Type"></param>
        /// <param name="openNode3ID"></param>
        /// <param name="openNode3Type"></param>
        /// <param name="displayItemID"></param>
        /// <param name="displayItemType"></param>
        /// <returns></returns>
		[WebMethod(Description="Returns the appropriate URL to the specified page.")]
		public string MakeLinkToPage ( string openSection, string organism, string openNode1ID, string openNode1Type, string openNode2ID, string openNode2Type, string openNode3ID, string openNode3Type, string displayItemID, string displayItemType )
		{
			string tail = LinkHelper.PrepareQueryString(
				openSection, organism,
				new Guid( openNode1ID ), openNode1Type,
				new Guid( openNode2ID ), openNode2Type,
				new Guid( openNode3ID ), openNode3Type,
				new Guid( displayItemID ), displayItemType
			);
			
			return "/PathwaysWeb/Web" + tail;
		}
		#endregion

        #region Gene Viewer Methods
        /// <summary>
        /// Returns the set of organisms that have at least one mapped gene encoding an enzyme of a given pathway
        /// </summary>
        /// <returns></returns>
        [WebMethod(Description = "Returns the set of organisms that have at least one mapped gene encoding an enzyme of a given pathway")]
        public string GetGenomesForPathway(string pathwayId)
        {
            try
            {
                OpenDB();
                Guid pwId = new Guid(pathwayId);
                return ServerPathway.GetGenomes(pwId);
                //return "";
            }
            catch (Exception e)
            {
                return e.Message + "\n" + e.StackTrace;
            }
            finally
            {
                CloseDB();
            }
        }

        /// <summary>
        /// Returns the set of genes that encodes the enzymes of a given pathway in XML.
        /// </summary>
        /// <returns></returns>
        [WebMethod(Description = "Returns the set of genes that encodes the enzymes of a given pathway.")]
        public string GetGeneMappingForPathway(string pathwayId)
        {
            try
            {
                OpenDB();
                Guid pwId = new Guid(pathwayId);
                return ServerPathway.GetGeneMapping(pwId);
                //return "";
            }
            catch (Exception e)
            {
                return e.Message + "\n" + e.StackTrace;
            }
            finally
            {
                CloseDB();
            }
        }

        [WebMethod(Description = "Returns the set of organisms that have at least one mapped gene encoding an enzyme of a pathway in a given set of comma separated pathways.")]
        public string GetGenomesForPathways(string pathwayIdSet)
        {
            try
            {
                if (pathwayIdSet.Trim().Length == 0)
                    return "";
                OpenDB();

                /** Older implementation for single pathway
                
                Guid pwId = new Guid(pathwayId);
                return ServerPathway.GetGenomes(pwId);
                 
                ***/
                return ServerPathway.GetGenomesForASetOfPathways(pathwayIdSet);
            }
            catch (Exception e)
            {
                return e.Message + "\n" + e.StackTrace;
            }
            finally
            {
                CloseDB();
            }
        }

        /// <summary>
        /// Returns the set of genes that encodes the enzymes of a given pathway in an organism.
        /// </summary>
        /// <returns></returns>
        [WebMethod(Description = "Returns the set of genes that encodes the enzymes of a given pathway in an organism.")]
        public string GetGeneMappingForOrganismPathway(string pathwayId, string organismGroupId)
        {
            try
            {
                OpenDB();
                Guid pwId = new Guid(pathwayId);
                Guid orgId = new Guid(organismGroupId);
                return ServerPathway.GetGeneMapping(pwId, orgId);
                //return "";
            }
            catch (Exception e)
            {
                return "";
            }
            finally
            {
                CloseDB();
            }
        }
        /// <summary>
        /// Returns the set of genes that encodes the enzymes of a given list of comma-separated pathways in an organism.
        /// </summary>
        /// <returns></returns>
        [WebMethod(Description = "Returns the set of genes that encode the enzymes of a given list of comma-separated pathways in an organism.")]
        public string GetGeneMappingForOrganismPathways(string pathwayIdSet, string organismGroupId)
        {
            try
            {
                if (pathwayIdSet.Trim().Length == 0)
                    return "";
                OpenDB();
                Guid orgId = new Guid(organismGroupId);

                /** Older implementation
                 
                Guid pwId = new Guid(pathwayId);                
                return ServerPathway.GetGeneMapping(pwId, orgId);
                
                **/

                return ServerPathway.GetGeneMappingForASetOfPathways(pathwayIdSet, orgId);

            }
            catch (Exception e)
            {
                return "";
            }
            finally
            {
                CloseDB();
            }
        }       

        #endregion

        #region //adding web services to get System Biology Models           
                
        /// <summary>
        /// Returns SB Model(by id) related all information, including reactions,compartments, etc.
        /// 03/09 Xinjian Qi
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [WebMethod(Description = "Returns SB Model realted all information.")] //, XmlInclude(typeof(ServerModel))
        public string GetSBModelByID(string id, string testid)
        {
            //return "Test Web Service string.";
            if (testid == null) return "test id is null";
            if (id == null) return "id is null";
            else  return "id and test id are not null, id is: "+ id;
            try
            {
                string xmlString = null;
                if (id.Length == 0) return "Empty id string";
                Guid mid = new Guid(id);                
                
                OpenDB();

                GraphServerObjectCache cache = new GraphServerObjectCache();
                //cache.g
                //ServerCompartment cm = new ServerCompartment();
                //ServerModel s = new ServerModel();
                //1. get compartment ids;reactions ids;kineticlaw ids in the model;  ServerModel.GetAllCompartmentIDs,
                //2. fill id and corresponding objects into cace object
                //3. call XMLDocumentToString
                //4. return string
                
                //return (ServerModel)ServerModel.Load(gid);
                //return (string[])compIDs.ToArray(typeof(string));

                if (!cache.HasModel(mid))
                {
                    //    Guid id = GraphNodeManager.GetEntityId(gid);

                    if (ServerModel.Exists(mid))
                        cache.FillCacheForModel(mid);
                }                        
                
                ArrayList compIDs = ServerCompartment.GetCompartmentIDsByModel(mid);

                if (compIDs != null && compIDs.Count > 0)
                {
                    foreach (object cid in compIDs)
                    {
                        if (!cache.HasCompartment(new Guid(cid.ToString())))
                        {
                        //    Guid id = GraphNodeManager.GetEntityId(gid);

                            if (ServerCompartment.Exists(new Guid(cid.ToString())))
                                cache.FillCacheForCompartment(new Guid(cid.ToString()));
                        }                        
                    }
                }

                ArrayList reacIDs = ServerReaction.GetReactionIDsByModel(mid);

                if (reacIDs != null && reacIDs.Count > 0)
                {
                    foreach (object rid in reacIDs)
                    {
                        if (!cache.HasReacion(new Guid(rid.ToString())))
                        {
                            //    Guid id = GraphNodeManager.GetEntityId(gid);

                            if (ServerReaction.Exists(new Guid(rid.ToString())))
                                cache.FillCacheForReaction(new Guid(rid.ToString()));
                        }
                    }
                }
                                
                xmlString = Util.XmlDocumentToString(cache.GenerateSBModelXml());
                return xmlString;
            }
            catch(Exception e)
            {
                return e.ToString();
                //return null;
            }
            finally
            {
                CloseDB();
            }
        }



        /*[WebMethod(Description = "Get data required to render a graph displaying the given set of objects and their interactions.  Each parameter accepts a semi-colon delimited list of object GUIDs of the appropriate type (or an empty string).")]
        public string GetGraphData(string collapsedPathwayGuids, string expandedPathwayGuids, string genericProcessGuids, string moleculeGuids)
        {
            try
            {
                string xmlString = null;
                Guid[] collapsedPathways = Util.GuidListFromString(collapsedPathwayGuids);
                Guid[] expandedPathways = Util.GuidListFromString(expandedPathwayGuids);
                Guid[] genericProcessesGraphNodes = Util.GuidListFromString(genericProcessGuids);
                Guid[] moleculeGraphNodes = Util.GuidListFromString(moleculeGuids);

                if (collapsedPathways.Length < 1 && expandedPathways.Length < 1 && genericProcessesGraphNodes.Length < 1 && moleculeGraphNodes.Length < 1)
                {
                    return "<ERROR message=\"Invalid parameters!\">Please specify the GUID of at least one entity to draw on the graph.</ERROR>";
                }

                OpenDB();

                GraphServerObjectCache cache = new GraphServerObjectCache();

                if (moleculeGraphNodes != null && moleculeGraphNodes.Length > 0)
                {
                    foreach (Guid gid in moleculeGraphNodes)
                    {
                        if (!cache.HasMolecule(gid))
                        {
                            Guid id = GraphNodeManager.GetEntityId(gid);

                            if (ServerMolecularEntity.Exists(id))
                                cache.FillCacheForMolecularEntity(gid);
                        }
                    }
                }

                if (expandedPathways != null && expandedPathways.Length > 0)
                {
                    foreach (Guid id in expandedPathways)
                    {
                        if (!cache.HasExpandedPathway(id))
                        {
                            if (ServerPathway.Exists(id))
                                cache.FillCacheForExpandedPathway(id);
                        }
                    }
                }

                if (collapsedPathways != null && collapsedPathways.Length > 0)
                {
                    foreach (Guid id in collapsedPathways)
                    {
                        if (!cache.HasCollapsedPathway(id))
                        {
                            if (ServerPathway.Exists(id))
                                cache.FillCacheForCollapsedPathway(id);
                        }
                    }
                }

                if (genericProcessesGraphNodes != null && genericProcessesGraphNodes.Length > 0)
                {
                    foreach (Guid gid in genericProcessesGraphNodes)
                    {
                        if (!cache.HasGenericProcess(gid))
                        {
                            Guid id = GraphNodeManager.GetGenericProcessId(gid);

                            if (ServerGenericProcess.Exists(id))
                                cache.FillCacheForGenericProcess(gid);
                        }
                    }
                }

                xmlString = Util.XmlDocumentToString(cache.GenerateDataXml());

                return xmlString;
            }
            catch (System.ArgumentException ae)
            {
                EventLogger.SystemEventLog("PathwaysService - MakeGraph", "Argument error in MakeGraph: " + ae);
                return "<ERROR message=\"Format Error!\">" + ae.Message + "</ERROR>";
            }
            catch (System.FormatException fe)
            {
                EventLogger.SystemEventLog("PathwaysService - MakeGraph", "Format error in MakeGraph: " + fe);
                return "<ERROR message=\"Format Error!\">" + fe.Message + "</ERROR>";
            }
            catch (Exception e)
            {
                EventLogger.SystemEventLog("PathwaysService - MakeGraph", "Exception in MakeGraph: " + e);
                return "<ERROR message=\"Unexpected Exception!\">" + e.ToString() + "</ERROR>";
            }
            finally
            {
                CloseDB();
            }

        }
        */
        #endregion


    }


	static class SBMLTest
	{
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		public static void Main(string[] args)
		{
			if (args.Length < 1)
			{
				Console.WriteLine("usage: SBMLTest <fileName>");
				System.Environment.Exit(-1);
			}

			// read the given SBML model
			SBMLReader oReader = new SBMLReader();
            SBMLDocument oDocument = oReader.readSBMLFromString("<?xml version=\"1.0\" encoding=\"UTF-8\"?> <sbml xmlns=\"http://www.sbml.org/sbml/level2\" metaid=\"_492719\" level=\"2\" version=\"1\">   <model metaid=\"_000001\" id=\"Kholodenko2000_MAPK_feedback\">     <notes>       <body xmlns=\"http://www.w3.org/1999/xhtml\">         <p>This model originates from BioModels Database: A Database of Annotated Published Models. It is copyright (c) 2005-2008 The BioModels Team.                  <br/>For more information see the                   <a href=\"http://www.ebi.ac.uk/biomodels/legal.html\" target=\"_blank\">terms of use</a>.                  <br/>To cite BioModels Database, please use                   <a href=\"http://www.pubmedcentral.nih.gov/articlerender.fcgi?tool=pubmed&amp;pubmedid=16381960\" target=\"_blank\"> Le Nov�re N., Bornstein B., Broicher A., Courtot M., Donizelli M., Dharuri H., Li L., Sauro H., Schilstra M., Shapiro B., Snoep J.L., Hucka M. (2006) BioModels Database: A Free, Centralized Database of Curated, Published, Quantitative Kinetic Models of Biochemical and Cellular Systems Nucleic Acids Res., 34: D689-D691.</a>       </p>     </body>   </notes>   <annotation>     <rdf:RDF xmlns:rdf=\"http://www.w3.org/1999/02/22-rdf-syntax-ns#\" xmlns:dc=\"http://purl.org/dc/elements/1.1/\" xmlns:dcterms=\"http://purl.org/dc/terms/\" xmlns:vCard=\"http://www.w3.org/2001/vcard-rdf/3.0#\" xmlns:bqbiol=\"http://biomodels.net/biology-qualifiers/\" xmlns:bqmodel=\"http://biomodels.net/model-qualifiers/\">       <rdf:Description rdf:about=\"#_000001\">         <dc:creator rdf:parseType=\"Resource\">           <rdf:Bag>             <rdf:li rdf:parseType=\"Resource\">               <vCard:N rdf:parseType=\"Resource\">                 <vCard:Family>Sauro</vCard:Family>                 <vCard:Given>Herbert</vCard:Given>               </vCard:N>               <vCard:EMAIL>Herbert_Sauro@kgi.edu</vCard:EMAIL>               <vCard:ORG>                 <vCard:Orgname>Keck Graduate Institute</vCard:Orgname>               </vCard:ORG>             </rdf:li>           </rdf:Bag>         </dc:creator>         <dcterms:created rdf:parseType=\"Resource\">           <dcterms:W3CDTF>2005-02-12T00:18:12Z</dcterms:W3CDTF>         </dcterms:created>         <dcterms:modified rdf:parseType=\"Resource\">           <dcterms:W3CDTF>2008-08-21T11:35:55Z</dcterms:W3CDTF>         </dcterms:modified>         <bqmodel:is>           <rdf:Bag>             <rdf:li rdf:resource=\"urn:miriam:biomodels.db:BIOMD0000000010\"/>           </rdf:Bag>         </bqmodel:is>         <bqmodel:isDescribedBy>           <rdf:Bag>             <rdf:li rdf:resource=\"urn:miriam:pubmed:10712587\"/>           </rdf:Bag>         </bqmodel:isDescribedBy>         <bqbiol:isHomologTo>           <rdf:Bag>             <rdf:li rdf:resource=\"urn:miriam:reactome:REACT_634\"/>           </rdf:Bag>         </bqbiol:isHomologTo>         <bqbiol:isVersionOf>           <rdf:Bag>             <rdf:li rdf:resource=\"urn:miriam:obo.go:GO%3A0000165\"/>           </rdf:Bag>         </bqbiol:isVersionOf>         <bqbiol:is>           <rdf:Bag>             <rdf:li rdf:resource=\"urn:miriam:taxonomy:8355\"/>           </rdf:Bag>         </bqbiol:is>       </rdf:Description>     </rdf:RDF>   </annotation>   <listOfUnitDefinitions>     <unitDefinition metaid=\"metaid_0000022\" id=\"substance\" name=\"nanomole\">       <listOfUnits>         <unit kind=\"mole\" scale=\"-9\"/>       </listOfUnits>     </unitDefinition>   </listOfUnitDefinitions>   <listOfCompartments>     <compartment metaid=\"_584463\" id=\"uVol\" size=\"1\"/>   </listOfCompartments>   <listOfSpecies>     <species metaid=\"_584475\" id=\"MKKK\" name=\"MAPKKK\" compartment=\"uVol\" initialConcentration=\"90\">       <annotation>         <rdf:RDF xmlns:rdf=\"http://www.w3.org/1999/02/22-rdf-syntax-ns#\" xmlns:dc=\"http://purl.org/dc/elements/1.1/\" xmlns:dcterms=\"http://purl.org/dc/terms/\" xmlns:vCard=\"http://www.w3.org/2001/vcard-rdf/3.0#\" xmlns:bqbiol=\"http://biomodels.net/biology-qualifiers/\" xmlns:bqmodel=\"http://biomodels.net/model-qualifiers/\">           <rdf:Description rdf:about=\"#_584475\">             <bqbiol:isVersionOf>               <rdf:Bag>                 <rdf:li rdf:resource=\"urn:miriam:uniprot:P09560\"/>               </rdf:Bag>             </bqbiol:isVersionOf>           </rdf:Description>         </rdf:RDF>       </annotation>     </species>     <species metaid=\"_584495\" id=\"MKKK_P\" name=\"MAPKKK-P\" compartment=\"uVol\" initialConcentration=\"10\">       <annotation>         <rdf:RDF xmlns:rdf=\"http://www.w3.org/1999/02/22-rdf-syntax-ns#\" xmlns:dc=\"http://purl.org/dc/elements/1.1/\" xmlns:dcterms=\"http://purl.org/dc/terms/\" xmlns:vCard=\"http://www.w3.org/2001/vcard-rdf/3.0#\" xmlns:bqbiol=\"http://biomodels.net/biology-qualifiers/\" xmlns:bqmodel=\"http://biomodels.net/model-qualifiers/\">           <rdf:Description rdf:about=\"#_584495\">             <bqbiol:isVersionOf>               <rdf:Bag>                 <rdf:li rdf:resource=\"urn:miriam:uniprot:P09560\"/>               </rdf:Bag>             </bqbiol:isVersionOf>           </rdf:Description>         </rdf:RDF>       </annotation>     </species>     <species metaid=\"_584515\" id=\"MKK\" name=\"MAPKK\" compartment=\"uVol\" initialConcentration=\"280\">       <annotation>         <rdf:RDF xmlns:rdf=\"http://www.w3.org/1999/02/22-rdf-syntax-ns#\" xmlns:dc=\"http://purl.org/dc/elements/1.1/\" xmlns:dcterms=\"http://purl.org/dc/terms/\" xmlns:vCard=\"http://www.w3.org/2001/vcard-rdf/3.0#\" xmlns:bqbiol=\"http://biomodels.net/biology-qualifiers/\" xmlns:bqmodel=\"http://biomodels.net/model-qualifiers/\">           <rdf:Description rdf:about=\"#_584515\">             <bqbiol:isVersionOf>               <rdf:Bag>                 <rdf:li rdf:resource=\"urn:miriam:uniprot:Q05116\"/>               </rdf:Bag>             </bqbiol:isVersionOf>           </rdf:Description>         </rdf:RDF>       </annotation>     </species>     <species metaid=\"_584535\" id=\"MKK_P\" name=\"MAPKK-P\" compartment=\"uVol\" initialConcentration=\"10\">       <annotation>         <rdf:RDF xmlns:rdf=\"http://www.w3.org/1999/02/22-rdf-syntax-ns#\" xmlns:dc=\"http://purl.org/dc/elements/1.1/\" xmlns:dcterms=\"http://purl.org/dc/terms/\" xmlns:vCard=\"http://www.w3.org/2001/vcard-rdf/3.0#\" xmlns:bqbiol=\"http://biomodels.net/biology-qualifiers/\" xmlns:bqmodel=\"http://biomodels.net/model-qualifiers/\">           <rdf:Description rdf:about=\"#_584535\">             <bqbiol:isVersionOf>               <rdf:Bag>                 <rdf:li rdf:resource=\"urn:miriam:uniprot:Q05116\"/>               </rdf:Bag>             </bqbiol:isVersionOf>           </rdf:Description>         </rdf:RDF>       </annotation>     </species>     <species metaid=\"_584555\" id=\"MKK_PP\" name=\"MAPKK-PP\" compartment=\"uVol\" initialConcentration=\"10\">       <annotation>         <rdf:RDF xmlns:rdf=\"http://www.w3.org/1999/02/22-rdf-syntax-ns#\" xmlns:dc=\"http://purl.org/dc/elements/1.1/\" xmlns:dcterms=\"http://purl.org/dc/terms/\" xmlns:vCard=\"http://www.w3.org/2001/vcard-rdf/3.0#\" xmlns:bqbiol=\"http://biomodels.net/biology-qualifiers/\" xmlns:bqmodel=\"http://biomodels.net/model-qualifiers/\">           <rdf:Description rdf:about=\"#_584555\">             <bqbiol:isVersionOf>               <rdf:Bag>                 <rdf:li rdf:resource=\"urn:miriam:uniprot:Q05116\"/>               </rdf:Bag>             </bqbiol:isVersionOf>           </rdf:Description>         </rdf:RDF>       </annotation>     </species>     <species metaid=\"_584575\" id=\"MAPK\" name=\"MAPK\" compartment=\"uVol\" initialConcentration=\"280\">       <annotation>         <rdf:RDF xmlns:rdf=\"http://www.w3.org/1999/02/22-rdf-syntax-ns#\" xmlns:dc=\"http://purl.org/dc/elements/1.1/\" xmlns:dcterms=\"http://purl.org/dc/terms/\" xmlns:vCard=\"http://www.w3.org/2001/vcard-rdf/3.0#\" xmlns:bqbiol=\"http://biomodels.net/biology-qualifiers/\" xmlns:bqmodel=\"http://biomodels.net/model-qualifiers/\">           <rdf:Description rdf:about=\"#_584575\">             <bqbiol:isVersionOf>               <rdf:Bag>                 <rdf:li rdf:resource=\"urn:miriam:uniprot:P26696\"/>               </rdf:Bag>             </bqbiol:isVersionOf>           </rdf:Description>         </rdf:RDF>       </annotation>     </species>     <species metaid=\"_584595\" id=\"MAPK_P\" name=\"MAPK-P\" compartment=\"uVol\" initialConcentration=\"10\">       <annotation>         <rdf:RDF xmlns:rdf=\"http://www.w3.org/1999/02/22-rdf-syntax-ns#\" xmlns:dc=\"http://purl.org/dc/elements/1.1/\" xmlns:dcterms=\"http://purl.org/dc/terms/\" xmlns:vCard=\"http://www.w3.org/2001/vcard-rdf/3.0#\" xmlns:bqbiol=\"http://biomodels.net/biology-qualifiers/\" xmlns:bqmodel=\"http://biomodels.net/model-qualifiers/\">           <rdf:Description rdf:about=\"#_584595\">             <bqbiol:isVersionOf>               <rdf:Bag>                 <rdf:li rdf:resource=\"urn:miriam:uniprot:P26696\"/>               </rdf:Bag>             </bqbiol:isVersionOf>           </rdf:Description>         </rdf:RDF>       </annotation>     </species>     <species metaid=\"_584615\" id=\"MAPK_PP\" name=\"MAPK-PP\" compartment=\"uVol\" initialConcentration=\"10\">       <annotation>         <rdf:RDF xmlns:rdf=\"http://www.w3.org/1999/02/22-rdf-syntax-ns#\" xmlns:dc=\"http://purl.org/dc/elements/1.1/\" xmlns:dcterms=\"http://purl.org/dc/terms/\" xmlns:vCard=\"http://www.w3.org/2001/vcard-rdf/3.0#\" xmlns:bqbiol=\"http://biomodels.net/biology-qualifiers/\" xmlns:bqmodel=\"http://biomodels.net/model-qualifiers/\">           <rdf:Description rdf:about=\"#_584615\">             <bqbiol:isVersionOf>               <rdf:Bag>                 <rdf:li rdf:resource=\"urn:miriam:uniprot:P26696\"/>               </rdf:Bag>             </bqbiol:isVersionOf>           </rdf:Description>         </rdf:RDF>       </annotation>     </species>   </listOfSpecies>   <listOfReactions>     <reaction metaid=\"_584635\" id=\"J0\" name=\"MAPKKK activation\" reversible=\"false\">       <annotation>         <rdf:RDF xmlns:rdf=\"http://www.w3.org/1999/02/22-rdf-syntax-ns#\" xmlns:dc=\"http://purl.org/dc/elements/1.1/\" xmlns:dcterms=\"http://purl.org/dc/terms/\" xmlns:vCard=\"http://www.w3.org/2001/vcard-rdf/3.0#\" xmlns:bqbiol=\"http://biomodels.net/biology-qualifiers/\" xmlns:bqmodel=\"http://biomodels.net/model-qualifiers/\">           <rdf:Description rdf:about=\"#_584635\">             <bqbiol:isHomologTo>               <rdf:Bag>                 <rdf:li rdf:resource=\"urn:miriam:reactome:REACT_525\"/>               </rdf:Bag>             </bqbiol:isHomologTo>             <bqbiol:isVersionOf>               <rdf:Bag>                 <rdf:li rdf:resource=\"urn:miriam:ec-code:2.7.11.1\"/>                 <rdf:li rdf:resource=\"urn:miriam:obo.go:GO%3A0008349\"/>                 <rdf:li rdf:resource=\"urn:miriam:obo.go:GO%3A0000185\"/>               </rdf:Bag>             </bqbiol:isVersionOf>           </rdf:Description>         </rdf:RDF>       </annotation>       <listOfReactants>         <speciesReference species=\"MKKK\"/>       </listOfReactants>       <listOfProducts>         <speciesReference species=\"MKKK_P\"/>       </listOfProducts>       <listOfModifiers>         <modifierSpeciesReference species=\"MAPK_PP\"/>       </listOfModifiers>       <kineticLaw>         <math xmlns=\"http://www.w3.org/1998/Math/MathML\">           <apply>             <divide/>             <apply>               <times/>               <ci> uVol </ci>               <ci> V1 </ci>               <ci> MKKK </ci>             </apply>             <apply>               <times/>               <apply>                 <plus/>                 <cn type=\"integer\"> 1 </cn>                 <apply>                   <power/>                   <apply>                     <divide/>                     <ci> MAPK_PP </ci>                     <ci> Ki </ci>                   </apply>                   <ci> n </ci>                 </apply>               </apply>               <apply>                 <plus/>                 <ci> K1 </ci>                 <ci> MKKK </ci>               </apply>             </apply>           </apply>         </math>         <listOfParameters>           <parameter id=\"V1\" value=\"2.5\"/>           <parameter id=\"Ki\" value=\"9\"/>           <parameter id=\"n\" value=\"1\"/>           <parameter id=\"K1\" value=\"10\"/>         </listOfParameters>       </kineticLaw>     </reaction>     <reaction metaid=\"_584655\" id=\"J1\" name=\"MAPKKK inactivation\" reversible=\"false\">       <annotation>         <rdf:RDF xmlns:rdf=\"http://www.w3.org/1999/02/22-rdf-syntax-ns#\" xmlns:dc=\"http://purl.org/dc/elements/1.1/\" xmlns:dcterms=\"http://purl.org/dc/terms/\" xmlns:vCard=\"http://www.w3.org/2001/vcard-rdf/3.0#\" xmlns:bqbiol=\"http://biomodels.net/biology-qualifiers/\" xmlns:bqmodel=\"http://biomodels.net/model-qualifiers/\">           <rdf:Description rdf:about=\"#_584655\">             <bqbiol:isVersionOf>               <rdf:Bag>                 <rdf:li rdf:resource=\"urn:miriam:ec-code:3.1.3.16\"/>                 <rdf:li rdf:resource=\"urn:miriam:obo.go:GO%3A0006470\"/>                 <rdf:li rdf:resource=\"urn:miriam:obo.go:GO%3A0051390\"/>               </rdf:Bag>             </bqbiol:isVersionOf>           </rdf:Description>         </rdf:RDF>       </annotation>       <listOfReactants>         <speciesReference species=\"MKKK_P\"/>       </listOfReactants>       <listOfProducts>         <speciesReference species=\"MKKK\"/>       </listOfProducts>       <kineticLaw>         <math xmlns=\"http://www.w3.org/1998/Math/MathML\">           <apply>             <divide/>             <apply>               <times/>               <ci> uVol </ci>               <ci> V2 </ci>               <ci> MKKK_P </ci>             </apply>             <apply>               <plus/>               <ci> KK2 </ci>               <ci> MKKK_P </ci>             </apply>           </apply>         </math>         <listOfParameters>           <parameter id=\"V2\" value=\"0.25\"/>           <parameter id=\"KK2\" value=\"8\"/>         </listOfParameters>       </kineticLaw>     </reaction>     <reaction metaid=\"_584675\" id=\"J2\" name=\"phosphorylation of MAPKK\" reversible=\"false\">       <annotation>         <rdf:RDF xmlns:rdf=\"http://www.w3.org/1999/02/22-rdf-syntax-ns#\" xmlns:dc=\"http://purl.org/dc/elements/1.1/\" xmlns:dcterms=\"http://purl.org/dc/terms/\" xmlns:vCard=\"http://www.w3.org/2001/vcard-rdf/3.0#\" xmlns:bqbiol=\"http://biomodels.net/biology-qualifiers/\" xmlns:bqmodel=\"http://biomodels.net/model-qualifiers/\">           <rdf:Description rdf:about=\"#_584675\">             <bqbiol:isHomologTo>               <rdf:Bag>                 <rdf:li rdf:resource=\"urn:miriam:reactome:REACT_614\"/>               </rdf:Bag>             </bqbiol:isHomologTo>             <bqbiol:isVersionOf>               <rdf:Bag>                 <rdf:li rdf:resource=\"urn:miriam:ec-code:2.7.11.25\"/>                 <rdf:li rdf:resource=\"urn:miriam:obo.go:GO%3A0006468\"/>                 <rdf:li rdf:resource=\"urn:miriam:obo.go:GO%3A0004709\"/>               </rdf:Bag>             </bqbiol:isVersionOf>           </rdf:Description>         </rdf:RDF>       </annotation>       <listOfReactants>         <speciesReference species=\"MKK\"/>       </listOfReactants>       <listOfProducts>         <speciesReference species=\"MKK_P\"/>       </listOfProducts>       <listOfModifiers>         <modifierSpeciesReference species=\"MKKK_P\"/>       </listOfModifiers>       <kineticLaw>         <math xmlns=\"http://www.w3.org/1998/Math/MathML\">           <apply>             <divide/>             <apply>               <times/>               <ci> uVol </ci>               <ci> k3 </ci>               <ci> MKKK_P </ci>               <ci> MKK </ci>             </apply>             <apply>               <plus/>               <ci> KK3 </ci>               <ci> MKK </ci>             </apply>           </apply>         </math>         <listOfParameters>           <parameter id=\"k3\" value=\"0.025\"/>           <parameter id=\"KK3\" value=\"15\"/>         </listOfParameters>       </kineticLaw>     </reaction>     <reaction metaid=\"_584695\" id=\"J3\" name=\"phosphorylation of MAPKK-P\" reversible=\"false\">       <annotation>         <rdf:RDF xmlns:rdf=\"http://www.w3.org/1999/02/22-rdf-syntax-ns#\" xmlns:dc=\"http://purl.org/dc/elements/1.1/\" xmlns:dcterms=\"http://purl.org/dc/terms/\" xmlns:vCard=\"http://www.w3.org/2001/vcard-rdf/3.0#\" xmlns:bqbiol=\"http://biomodels.net/biology-qualifiers/\" xmlns:bqmodel=\"http://biomodels.net/model-qualifiers/\">           <rdf:Description rdf:about=\"#_584695\">             <bqbiol:isVersionOf>               <rdf:Bag>                 <rdf:li rdf:resource=\"urn:miriam:ec-code:2.7.11.25\"/>                 <rdf:li rdf:resource=\"urn:miriam:obo.go:GO%3A0004709\"/>                 <rdf:li rdf:resource=\"urn:miriam:obo.go:GO%3A0006468\"/>                 <rdf:li rdf:resource=\"urn:miriam:obo.go:GO%3A0000186\"/>               </rdf:Bag>             </bqbiol:isVersionOf>             <bqbiol:isHomologTo>               <rdf:Bag>                 <rdf:li rdf:resource=\"urn:miriam:reactome:REACT_614\"/>               </rdf:Bag>             </bqbiol:isHomologTo>           </rdf:Description>         </rdf:RDF>       </annotation>       <listOfReactants>         <speciesReference species=\"MKK_P\"/>       </listOfReactants>       <listOfProducts>         <speciesReference species=\"MKK_PP\"/>       </listOfProducts>       <listOfModifiers>         <modifierSpeciesReference species=\"MKKK_P\"/>       </listOfModifiers>       <kineticLaw>         <math xmlns=\"http://www.w3.org/1998/Math/MathML\">           <apply>             <divide/>             <apply>               <times/>               <ci> uVol </ci>               <ci> k4 </ci>               <ci> MKKK_P </ci>               <ci> MKK_P </ci>             </apply>             <apply>               <plus/>               <ci> KK4 </ci>               <ci> MKK_P </ci>             </apply>           </apply>         </math>         <listOfParameters>           <parameter id=\"k4\" value=\"0.025\"/>           <parameter id=\"KK4\" value=\"15\"/>         </listOfParameters>       </kineticLaw>     </reaction>     <reaction metaid=\"_584715\" id=\"J4\" name=\"dephosphorylation of MAPKK-PP\" reversible=\"false\">       <annotation>         <rdf:RDF xmlns:rdf=\"http://www.w3.org/1999/02/22-rdf-syntax-ns#\" xmlns:dc=\"http://purl.org/dc/elements/1.1/\" xmlns:dcterms=\"http://purl.org/dc/terms/\" xmlns:vCard=\"http://www.w3.org/2001/vcard-rdf/3.0#\" xmlns:bqbiol=\"http://biomodels.net/biology-qualifiers/\" xmlns:bqmodel=\"http://biomodels.net/model-qualifiers/\">           <rdf:Description rdf:about=\"#_584715\">             <bqbiol:isVersionOf>               <rdf:Bag>                 <rdf:li rdf:resource=\"urn:miriam:ec-code:3.1.3.16\"/>                 <rdf:li rdf:resource=\"urn:miriam:obo.go:GO%3A0051389\"/>                 <rdf:li rdf:resource=\"urn:miriam:obo.go:GO%3A0006470\"/>               </rdf:Bag>             </bqbiol:isVersionOf>           </rdf:Description>         </rdf:RDF>       </annotation>       <listOfReactants>         <speciesReference species=\"MKK_PP\"/>       </listOfReactants>       <listOfProducts>         <speciesReference species=\"MKK_P\"/>       </listOfProducts>       <kineticLaw>         <math xmlns=\"http://www.w3.org/1998/Math/MathML\">           <apply>             <divide/>             <apply>               <times/>               <ci> uVol </ci>               <ci> V5 </ci>               <ci> MKK_PP </ci>             </apply>             <apply>               <plus/>               <ci> KK5 </ci>               <ci> MKK_PP </ci>             </apply>           </apply>         </math>         <listOfParameters>           <parameter id=\"V5\" value=\"0.75\"/>           <parameter id=\"KK5\" value=\"15\"/>         </listOfParameters>       </kineticLaw>     </reaction>     <reaction metaid=\"_584735\" id=\"J5\" name=\"dephosphorylation of MAPKK-P\" reversible=\"false\">       <annotation>         <rdf:RDF xmlns:rdf=\"http://www.w3.org/1999/02/22-rdf-syntax-ns#\" xmlns:dc=\"http://purl.org/dc/elements/1.1/\" xmlns:dcterms=\"http://purl.org/dc/terms/\" xmlns:vCard=\"http://www.w3.org/2001/vcard-rdf/3.0#\" xmlns:bqbiol=\"http://biomodels.net/biology-qualifiers/\" xmlns:bqmodel=\"http://biomodels.net/model-qualifiers/\">           <rdf:Description rdf:about=\"#_584735\">             <bqbiol:isVersionOf>               <rdf:Bag>                 <rdf:li rdf:resource=\"urn:miriam:ec-code:3.1.3.16\"/>                 <rdf:li rdf:resource=\"urn:miriam:obo.go:GO%3A0006470\"/>               </rdf:Bag>             </bqbiol:isVersionOf>           </rdf:Description>         </rdf:RDF>       </annotation>       <listOfReactants>         <speciesReference species=\"MKK_P\"/>       </listOfReactants>       <listOfProducts>         <speciesReference species=\"MKK\"/>       </listOfProducts>       <kineticLaw>         <math xmlns=\"http://www.w3.org/1998/Math/MathML\">           <apply>             <divide/>             <apply>               <times/>               <ci> uVol </ci>               <ci> V6 </ci>               <ci> MKK_P </ci>             </apply>             <apply>               <plus/>               <ci> KK6 </ci>               <ci> MKK_P </ci>             </apply>           </apply>         </math>         <listOfParameters>           <parameter id=\"V6\" value=\"0.75\"/>           <parameter id=\"KK6\" value=\"15\"/>         </listOfParameters>       </kineticLaw>     </reaction>     <reaction metaid=\"_584755\" id=\"J6\" name=\"phosphorylation of MAPK\" reversible=\"false\">       <annotation>         <rdf:RDF xmlns:rdf=\"http://www.w3.org/1999/02/22-rdf-syntax-ns#\" xmlns:dc=\"http://purl.org/dc/elements/1.1/\" xmlns:dcterms=\"http://purl.org/dc/terms/\" xmlns:vCard=\"http://www.w3.org/2001/vcard-rdf/3.0#\" xmlns:bqbiol=\"http://biomodels.net/biology-qualifiers/\" xmlns:bqmodel=\"http://biomodels.net/model-qualifiers/\">           <rdf:Description rdf:about=\"#_584755\">             <bqbiol:hasVersion>               <rdf:Bag>                 <rdf:li rdf:resource=\"urn:miriam:reactome:REACT_2247\"/>                 <rdf:li rdf:resource=\"urn:miriam:reactome:REACT_136\"/>               </rdf:Bag>             </bqbiol:hasVersion>             <bqbiol:isVersionOf>               <rdf:Bag>                 <rdf:li rdf:resource=\"urn:miriam:ec-code:2.7.12.2\"/>                 <rdf:li rdf:resource=\"urn:miriam:obo.go:GO%3A0006468\"/>                 <rdf:li rdf:resource=\"urn:miriam:obo.go:GO%3A0004708\"/>               </rdf:Bag>             </bqbiol:isVersionOf>           </rdf:Description>         </rdf:RDF>       </annotation>       <listOfReactants>         <speciesReference species=\"MAPK\"/>       </listOfReactants>       <listOfProducts>         <speciesReference species=\"MAPK_P\"/>       </listOfProducts>       <listOfModifiers>         <modifierSpeciesReference species=\"MKK_PP\"/>       </listOfModifiers>       <kineticLaw>         <math xmlns=\"http://www.w3.org/1998/Math/MathML\">           <apply>             <divide/>             <apply>               <times/>               <ci> uVol </ci>               <ci> k7 </ci>               <ci> MKK_PP </ci>               <ci> MAPK </ci>             </apply>             <apply>               <plus/>               <ci> KK7 </ci>               <ci> MAPK </ci>             </apply>           </apply>         </math>         <listOfParameters>           <parameter id=\"k7\" value=\"0.025\"/>           <parameter id=\"KK7\" value=\"15\"/>         </listOfParameters>       </kineticLaw>     </reaction>     <reaction metaid=\"_584775\" id=\"J7\" name=\"phosphorylation of MAPK-P\" reversible=\"false\">       <annotation>         <rdf:RDF xmlns:rdf=\"http://www.w3.org/1999/02/22-rdf-syntax-ns#\" xmlns:dc=\"http://purl.org/dc/elements/1.1/\" xmlns:dcterms=\"http://purl.org/dc/terms/\" xmlns:vCard=\"http://www.w3.org/2001/vcard-rdf/3.0#\" xmlns:bqbiol=\"http://biomodels.net/biology-qualifiers/\" xmlns:bqmodel=\"http://biomodels.net/model-qualifiers/\">           <rdf:Description rdf:about=\"#_584775\">             <bqbiol:hasVersion>               <rdf:Bag>                 <rdf:li rdf:resource=\"urn:miriam:reactome:REACT_2247\"/>                 <rdf:li rdf:resource=\"urn:miriam:reactome:REACT_136\"/>               </rdf:Bag>             </bqbiol:hasVersion>             <bqbiol:isVersionOf>               <rdf:Bag>                 <rdf:li rdf:resource=\"urn:miriam:ec-code:2.7.12.2\"/>                 <rdf:li rdf:resource=\"urn:miriam:obo.go:GO%3A0000187\"/>                 <rdf:li rdf:resource=\"urn:miriam:obo.go:GO%3A0004708\"/>                 <rdf:li rdf:resource=\"urn:miriam:obo.go:GO%3A0006468\"/>               </rdf:Bag>             </bqbiol:isVersionOf>           </rdf:Description>         </rdf:RDF>       </annotation>       <listOfReactants>         <speciesReference species=\"MAPK_P\"/>       </listOfReactants>       <listOfProducts>         <speciesReference species=\"MAPK_PP\"/>       </listOfProducts>       <listOfModifiers>         <modifierSpeciesReference species=\"MKK_PP\"/>       </listOfModifiers>       <kineticLaw>         <math xmlns=\"http://www.w3.org/1998/Math/MathML\">           <apply>             <divide/>             <apply>               <times/>               <ci> uVol </ci>               <ci> k8 </ci>               <ci> MKK_PP </ci>               <ci> MAPK_P </ci>             </apply>             <apply>               <plus/>               <ci> KK8 </ci>               <ci> MAPK_P </ci>             </apply>           </apply>         </math>         <listOfParameters>           <parameter id=\"k8\" value=\"0.025\"/>           <parameter id=\"KK8\" value=\"15\"/>         </listOfParameters>       </kineticLaw>     </reaction>     <reaction metaid=\"_584795\" id=\"J8\" name=\"dephosphorylation of MAPK-PP\" reversible=\"false\">       <annotation>         <rdf:RDF xmlns:rdf=\"http://www.w3.org/1999/02/22-rdf-syntax-ns#\" xmlns:dc=\"http://purl.org/dc/elements/1.1/\" xmlns:dcterms=\"http://purl.org/dc/terms/\" xmlns:vCard=\"http://www.w3.org/2001/vcard-rdf/3.0#\" xmlns:bqbiol=\"http://biomodels.net/biology-qualifiers/\" xmlns:bqmodel=\"http://biomodels.net/model-qualifiers/\">           <rdf:Description rdf:about=\"#_584795\">             <bqbiol:isVersionOf>               <rdf:Bag>                 <rdf:li rdf:resource=\"urn:miriam:ec-code:3.1.3.16\"/>                 <rdf:li rdf:resource=\"urn:miriam:obo.go:GO%3A0006470\"/>                 <rdf:li rdf:resource=\"urn:miriam:obo.go:GO%3A0000188\"/>               </rdf:Bag>             </bqbiol:isVersionOf>           </rdf:Description>         </rdf:RDF>       </annotation>       <listOfReactants>         <speciesReference species=\"MAPK_PP\"/>       </listOfReactants>       <listOfProducts>         <speciesReference species=\"MAPK_P\"/>       </listOfProducts>       <kineticLaw>         <math xmlns=\"http://www.w3.org/1998/Math/MathML\">           <apply>             <divide/>             <apply>               <times/>               <ci> uVol </ci>               <ci> V9 </ci>               <ci> MAPK_PP </ci>             </apply>             <apply>               <plus/>               <ci> KK9 </ci>               <ci> MAPK_PP </ci>             </apply>           </apply>         </math>         <listOfParameters>           <parameter id=\"V9\" value=\"0.5\"/>           <parameter id=\"KK9\" value=\"15\"/>         </listOfParameters>       </kineticLaw>     </reaction>     <reaction metaid=\"_584815\" id=\"J9\" name=\"dephosphorylation of MAPK-P\" reversible=\"false\">       <annotation>         <rdf:RDF xmlns:rdf=\"http://www.w3.org/1999/02/22-rdf-syntax-ns#\" xmlns:dc=\"http://purl.org/dc/elements/1.1/\" xmlns:dcterms=\"http://purl.org/dc/terms/\" xmlns:vCard=\"http://www.w3.org/2001/vcard-rdf/3.0#\" xmlns:bqbiol=\"http://biomodels.net/biology-qualifiers/\" xmlns:bqmodel=\"http://biomodels.net/model-qualifiers/\">           <rdf:Description rdf:about=\"#_584815\">             <bqbiol:isVersionOf>               <rdf:Bag>                 <rdf:li rdf:resource=\"urn:miriam:ec-code:3.1.3.16\"/>                 <rdf:li rdf:resource=\"urn:miriam:obo.go:GO%3A0006470\"/>               </rdf:Bag>             </bqbiol:isVersionOf>           </rdf:Description>         </rdf:RDF>       </annotation>       <listOfReactants>         <speciesReference species=\"MAPK_P\"/>       </listOfReactants>       <listOfProducts>         <speciesReference species=\"MAPK\"/>       </listOfProducts>       <kineticLaw>         <math xmlns=\"http://www.w3.org/1998/Math/MathML\">           <apply>             <divide/>             <apply>               <times/>               <ci> uVol </ci>               <ci> V10 </ci>               <ci> MAPK_P </ci>             </apply>             <apply>               <plus/>               <ci> KK10 </ci>               <ci> MAPK_P </ci>             </apply>           </apply>         </math>         <listOfParameters>           <parameter id=\"V10\" value=\"0.5\"/>           <parameter id=\"KK10\" value=\"15\"/>         </listOfParameters>       </kineticLaw>     </reaction>   </listOfReactions> </model> </sbml>");//oReader.readSBML(args[0]);
			Model oModel = oDocument.getModel();

			// print ID and Name
			Console.WriteLine(
					String.Format(
						"Id: '{0}' and Name: '{1}'{2}", 
						oModel.getId(), 
						oModel.getName(), Environment.NewLine)
				);

			// loop through species
			PrintSpecies(oModel);
			
			// loop through reactions and write script style reaction scheme
			PrintReactions(oModel);

			Console.WriteLine(String.Format("{0}<Hit <Return> to continue>{0}", Environment.NewLine));
			Console.ReadLine();

			return;

		}

		/// <summary>
		/// Loop through all species in the given model and print ID and initialConcentration
		/// </summary>
		/// <param name="oModel">the model</param>
		private static void PrintSpecies(Model oModel)
		{
			Console.WriteLine(String.Format("Number of Species: {0}", oModel.getNumSpecies()));
			for (int i = 0; i < oModel.getNumSpecies(); i++)
			{
				Species s = oModel.getSpecies(i);
				Console.WriteLine(String.Format("...id: {0} concentration: {1}", s.getId(), s.getInitialConcentration()));
			}
		}

		/// <summary>
		/// Loop through all reactions in the given model and print the reaction scheme.
		/// </summary>
		/// <param name="oModel">the model</param>
		private static void PrintReactions(Model oModel)
		{
			Console.WriteLine(String.Format("{1}Number of Reactions: {0}", oModel.getNumReactions(), Environment.NewLine));
			for (int n = 0; n < oModel.getNumReactions(); n++)
			{
				PrintNthReaction(oModel, n);
			}
		}

		/// <summary>
		/// Obtain the reaction with the given index and print its reaction scheme and kinetic law. 
		/// </summary>
		/// <param name="oModel">the model</param>
		/// <param name="index">index of the reaction</param>
		private static void PrintNthReaction(Model oModel, int index)
		{
			Reaction r = oModel.getReaction(index);
			Console.Write(String.Format("\t{0} : ", r.getId()));

            // Print lefthand side of the reaction scheme    
			for (int j = 0; j < r.getNumReactants(); j++)
			{
				SpeciesReference oRef = r.getReactant(j);
				Console.Write(
					String.Format("{0} {1} ", oRef.getStoichiometry(), oRef.getSpecies()) );
				if (j + 1 < r.getNumReactants())
					Console.Write(" + ");
				else
				{
					if (r.getReversible())
					{
						Console.Write(" => ");
					}
					else
					{
						Console.Write(" -> ");
					}
				}
			}

			// Print righthand side of the reaction scheme
			for (int j = 0; j < r.getNumProducts(); j++)
			{
				SpeciesReference oRef = r.getProduct(j);
				Console.Write(
					String.Format("{0} {1} ", oRef.getStoichiometry(), oRef.getSpecies()) );
				if (j + 1 < r.getNumReactants())
					Console.Write(" + ");
				else
					Console.Write(";" + Environment.NewLine);
			}

			// Print the kineticLaw
			Console.WriteLine(
				String.Format("\t{0};{1}",
				libsbml.libsbml.formulaToString(r.getKineticLaw().getMath()),
				Environment.NewLine
				));
		}
	}

}
