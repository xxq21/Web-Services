using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Services;
using System.Web.Script.Serialization;
using System.IO;
using XMLWebpad;

namespace PathwaysService
{
    /// <summary>
    /// This web service shows how to make use of AJAX call in order to interact with the XMLEditorToolKit server library
    /// 
    /// * XML Editor ToolKit Demo Webservice
    /// * Copyright (c) 20010 Udayan Das
    /// * Dual licensed under the MIT and GPL licenses.
    /// * http://xmleditortoolkit.codeplex.com/License
    /// * Date: 2010-06-20 (Sat, 20 Jun 2010)
    ///   
    /// </summary>
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    [System.Web.Script.Services.ScriptService]
    public class XMLEditorService : System.Web.Services.WebService
    {

        static List<Node> nodes;

        [WebMethod]
        public string SaveChanges(List<XMLWebpad.TreeNode> tree1, List<XMLWebpad.TreeNode> tree2, string xmlfileloc, UserAuthInfo userinfo)
        {
            TreeBuilder tb = new TreeBuilder();
            return tb.SaveChanges(tree1, tree2, xmlfileloc, userinfo);
        }

        [WebMethod]
        public string GetXMLMap(string webconfigloc, UserAuthInfo userinfo)
        {
            string _xmlmapJSON = "";
            JavaScriptSerializer js = new JavaScriptSerializer();
            TreeBuilder _builder = new TreeBuilder();

            try
            {
                nodes = _builder.BuildXmlString(webconfigloc);
                _xmlmapJSON = js.Serialize(nodes);
            }
            catch
            {
                try
                {
                    nodes = _builder.Build(webconfigloc);
                    _xmlmapJSON = js.Serialize(nodes);
                }
                catch (Exception appexp)
                {
                    throw (appexp);
                }
            }

            return (_xmlmapJSON);
        }
    }
}
