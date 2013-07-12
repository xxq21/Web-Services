using System;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Web;
using System.Web.Services;
using System.Configuration;
using System.Data.SqlClient;
using PathwaysLib.ServerObjects;

namespace GO2Pathways
{
	/// <sourcefile>
	///		<project>Pathways</project>
	///		<filepath>PathwaysService/GOPathwaysService.cs</filepath>
	///		<creation>2005/02/16</creation>
	///		<author>
	///			<name>Marc R. Reynolds</name>
	///			<initials>mrr</initials>
	///			<email>marc.reynolds@cwru.edu</email>
	///		</author>
	///		<cvs>
	///			<cvs_author>$Author: mustafa $</cvs_author>
	///			<cvs_date>$Date: 2008/05/16 21:15:58 $</cvs_date>
	///			<cvs_header>$Header: /var/lib/cvs/PathCase_SystemsBiology/PathwaysService/App_Code/GOPathwaysService.asmx.cs,v 1.1 2008/05/16 21:15:58 mustafa Exp $</cvs_header>
	///			<cvs_branch>$Name:  $</cvs_branch>
	///			<cvs_revision>$Revision: 1.1 $</cvs_revision>
	///		</cvs>
	///</sourcefile>
	/// <summary>
	/// Exposes services which provide information about linkages between GO terms and Pathways
	/// </summary>
	public class GO2PathwaysService : System.Web.Services.WebService
	{
		/*
		 * 
		 */ 
		private readonly static string CONNECTION_STRING = ConfigurationManager.AppSettings["dbConnectString"];
		// private PathwaysService.PathwaysService2.Service1 service1 = new PathwaysService.PathwaysService2.Service1(); // BLAST_THIS

		/// <summary>
		/// Creates a new instance of GO2PathwaysService
		/// 
		/// This class makes database queries to find links between GO terms, processes, and pathways.
		/// It can also generate certain statistiscal data.
		/// </summary>
		public GO2PathwaysService()
		{
			//CODEGEN: This call is required by the ASP.NET Web Services Designer
			InitializeComponent();
		}

		/// <summary>
		/// Retrieve GO Terms associated with a specific EC number
		/// </summary>
		/// <param name="ecNumber">The EC Number (e.g.: "EC 1.2.33.3")</param>
		/// <returns>The GO Terms associated with that EC Number</returns>
		/// <remarks>METHOD BEING USED - DO NOT CHANGE SIGNATURE
		/// Used in DisplayGODetail in PathwaysWeb
		/// </remarks>
		[WebMethod]
		public GORecord[] GetGORecordsFromECNumber(string ecNumber)
		{
			DataTable tableGO = GetGOFromECNumber(ecNumber);
			ArrayList listTerms = new ArrayList();
			foreach(DataRow rowGO in tableGO.Rows)
			{
				listTerms.Add(GORecord.FromDataRow(rowGO));
			}
			return (GORecord[])listTerms.ToArray(typeof(GORecord));
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="processID"></param>
		/// <param name="treeLevel"></param>
		/// <returns></returns>
		/// <remarks>METHOD BEING USED - DON'T CHANGE SIGNATURE</remarks>
		[WebMethod]
		public GORecord[] GetGORecordsFromProcessID_GOTreeLevel(Guid processID, int treeLevel)
		{
			//get the record from the max level and then walk up the tree until the level == treeLevel
			GORecord[] recs = GetGORecordsFromProcessID_MaxTreeLevel(processID);

			//if we're just looking for the max level, return what we have
			//without transposing
			if(treeLevel == int.MaxValue) return recs;

			string catalyticID = GetCatalyticActivityGOID();

			ArrayList listRecs=  new ArrayList();
			foreach(GORecord rec in recs)
				listRecs.AddRange(TransposeGOTermUpToLevel(rec, treeLevel));
			return (GORecord[])listRecs.ToArray(typeof(GORecord));
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="processID"></param>
		/// <returns></returns>
		/// <remarks>METHOD BEING USED - DON'T CHANGE SIGNATURE</remarks>
		[WebMethod]
		public GORecord[] GetGORecordsFromProcessID_MaxTreeLevel(Guid processID)
		{
			GORecord[] recs = GetGORecordsFromProcessID_EC(processID);
			return recs;
		}

		#region Helper Methods

		/// <summary>
		/// Returns the Max-Max Level this occurrs
		/// </summary>
		/// <param name="goID">The GO ID of the term</param>
		/// <returns>The Max-Max level</returns>
		[WebMethod]
		public int GetMaxMaxTreeLevelOfGOTerm(string goID)
		{
			SqlConnection con = new SqlConnection(CONNECTION_STRING);
			try
			{
				con.Open();
				SqlCommand com = new SqlCommand("select max(TermLevel) from Pubmed.dbo.GOTermsTree where ChildID='" + goID + "'", con);
				object o = com.ExecuteScalar();
				if(o == null) throw new ArgumentException("Invalid GOID", "goID");
				return (int)o;
			}
			catch(Exception se){throw se;}
			finally{con.Close();}
		}
		[WebMethod]
		public string GetCatalyticActivityGOID()
		{
			DataTable dt = ProcessCommand("select * from Pubmed.dbo.GOTerms where Name='Catalytic Activity'");
			if(dt.Rows.Count == 0) throw new ApplicationException("Could not find 'Catalytic Activity' term");
			else if(dt.Rows.Count > 1) throw new ApplicationException("Found more than one term 'Catalytic Activity'");
			else return (string)dt.Rows[0]["ID"];
		}
		private DataTable GetGOFromECNumber(string ecNumber)
		{
			//throw new NotSupportedException("DEPRECATED");
			try
			{
				string strCommand = 
					"select GOID=GO_ID, ECNumber=EC_Number from Pubmed.dbo.ec2GO "+
					"	where EC_Number = @ecNumber ";
				return ProcessCommand(strCommand, new SqlParameter("@ecNumber", ecNumber));
			}
			catch(Exception se)
			{
				throw new Exception("Error in GetGoFromECNumber", se);
			}
		}

		private DataTable ProcessCommand(string strCommand, params SqlParameter[] param)
		{
			using(SqlConnection con = new SqlConnection(CONNECTION_STRING))
			{
				try
				{
					con.Open();

					SqlCommand com = new SqlCommand(strCommand, con);
					if(param != null)
						foreach(SqlParameter p in param)
							com.Parameters.Add(p);

					DataTable dt = GetDataTableFromCommandString(com);

					return dt;
				}
				catch(Exception se)
				{
					throw new Exception("Error in ProcessCommand", se);
				}
				finally{con.Close();}
			}
		}

		private DataTable GetDataTableFromCommandString(SqlCommand com)
		{
			try
			{
				SqlDataAdapter da = new SqlDataAdapter(com);
				DataSet ds = new DataSet();
				da.Fill(ds);
				DataTable dt = ds.Tables[0];

				ds.Dispose();
				da.Dispose();

				return dt;
			}
			catch(Exception se)
			{
				throw new Exception("Error in GetDataAdapterFromCommandString", se);
			}
		}

		private DataTable GetGOFromProcessID_ECNumber(Guid processID)
		{
            //TODO: FIXME
            string variable = 
				"select GOName = GOTerms.[name], GOIDs.* from  "+
				"	(select distinct ECNumber=ec2go.EC_Number, gpp.process_name, gpp.process_ID, GOID=ec2go.GO_ID from "+
				"		(select gpp1.*, process_name=processes.name from (select * from "+
				"			[metabolic-1.0].dbo.gene_product_and_processes "+
				"			where process_id = @processID )gpp1 inner join [metabolic-1.0].dbo.processes processes on gpp1.process_id=processes.id "+
				"		) gpp "+
				"		inner join "+
				"		Pubmed.dbo.ec2go ec2go "+
				"		on gpp.ec_number = ec2go.EC_Number "+
				"	) GOIDs "+
				"	inner join "+
				"	Pubmed.dbo.GOTerms GOTerms "+
				"	on GOIDs.GOID = GOTerms.[ID] ";
			return ProcessCommand(variable, new SqlParameter("@processID", processID));
		}

		/// <summary>
		/// Returns GO Records associated with the given process
		/// </summary>
		/// <param name="processID">The Guid of rht eprocess we want to explore</param>
		/// <returns>An array of GORecords associated with the process</returns>
		[WebMethod]
		public GORecord[] GetGORecordsFromProcessID_EC(Guid processID)
		{
			//find the GO Terms through EC Numbers
			//			PathwaysLib.ServerObjects.ServerProcess sp = PathwaysLib.ServerObjects.ServerProcess.Load(processID);
			//			Guid gProcessID = sp.GenericProcessID;
			DataTable dtGO_EC = GetGOFromProcessID_ECNumber(processID);

			ArrayList goRecords = new ArrayList();
			foreach(DataRow dr in dtGO_EC.Rows)
			{
				GORecord rec = GORecord.FromDataRow(dr);
				rec.TreeLevel = GetMaxMaxTreeLevelOfGOTerm(rec.ID);
				goRecords.Add(rec);
			}
			return (GORecord[])goRecords.ToArray(typeof(GORecord));
		}

		public int GetMinTreeLevelOfGOTerm_UnderCatalyticActivity(string goID)
		{
			SqlConnection con = new SqlConnection(CONNECTION_STRING);
			try
			{
				con.Open();
				SqlCommand com = new SqlCommand("select max(TermLevel) from Pubmed.dbo.GOTermsTree where ChildID='" + goID + "' and OnPathUnderCatalyticActivity=1", con);
				object o = com.ExecuteScalar();
				if(o == null) throw new ArgumentException("Invalid GOID", "goID");
				return (int)o;
			}
			catch(Exception se){throw se;}
			finally{con.Close();}
		}

		[WebMethod]
		public GORecord[] TransposeGOTermUpToLevel(GORecord record, int treeLevel)
		{
			if(GetMinTreeLevelOfGOTerm_UnderCatalyticActivity(record.ID) <= treeLevel) return new GORecord[]{record};

			//a queue to aid in BFS
			Queue goTerms = new Queue(new GORecord[]{record});

			ArrayList listLevelXAnnotations = new ArrayList();

			//keep track of all categories so we don't go up a path multiple times
			ArrayList listTouchedCategories = new ArrayList();
			while(goTerms.Count > 0)
			{
				GORecord rec = (GORecord)goTerms.Dequeue();

				//if this is at the correct level or above, add it to the annotation list
				int minLevel = GetMaxMaxTreeLevelOfGOTerm(rec.ID);
				if(minLevel <= treeLevel)
				{
					rec.TreeLevel = minLevel;
					listLevelXAnnotations.Add(rec);
				}
				else
				{
					//find it's parents who are under Catalytic Activity and add those to the Queue
					SqlConnection con = new SqlConnection(CONNECTION_STRING);
					try
					{
						con.Open();

						string command = 
							"select ParentName = terms.[Name], tt.* from "+
							"	(select * from Pubmed.dbo.GOTermsTree where ChildID=@childID and OnPathUnderCatalyticActivity=1) tt "+
							"	inner join "+
							"	Pubmed.dbo.GOTerms terms "+
							"	on tt.ParentID = terms.[ID] ";

						SqlCommand com = new SqlCommand(command, con);
						com.Parameters.Add("@childID", rec.ID);
						SqlDataReader dr = com.ExecuteReader();
						while(dr.Read())
						{
							//add a modified version of this same record to the queue
							string GOID = (string)dr["ParentID"];
							string GOName = (string)dr["ParentName"];
							int nextLevel = (int)dr["TermLevel"];
							GORecord newRec = new GORecord();
							newRec.ECNumber = rec.ECNumber;
							newRec.Notes = rec.Notes;
							newRec.ProcessID = rec.ProcessID;
							newRec.ProcessName = rec.ProcessName;
							newRec.TreeLevel = nextLevel;
							newRec.ID = GOID;
							newRec.Name = GOName;

							//only add this category if we haven't been up this path
							if(!listTouchedCategories.Contains(newRec.ID))
							{
								goTerms.Enqueue(newRec);
								listTouchedCategories.Add(newRec.ID);
							}
						}
						dr.Close();
					}
					catch(Exception se){throw se;}
					finally{con.Close();}
				}
			}

			return (GORecord[])listLevelXAnnotations.ToArray(typeof(GORecord));

		}
		#endregion

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

	}
}
