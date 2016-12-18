using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using Tie;

namespace UnitTest
{
    class JSONTest
    {
        public static void main()
        {
            string code1 = @"
{
    'glossary': {
        'title': 'example glossary',
		'GlossDiv': {
            'title': 'S',
			'GlossList': {
                'GlossEntry': {
                    'ID': 'SGML',
					'SortAs': 'SGML',
					'GlossTerm': 'Standard Generalized Markup Language',
					'Acronym': 'SGML',
					'Abbrev': 'ISO 8879:1986',
					'GlossDef': {
                        'para': 'A meta-markup language, used to create markup languages such as DocBook.',
						'GlossSeeAlso': ['GML', 'XML']
                    },
					'GlossSee': 'markup'
                }
            }
        }
    }
}

";

            string code2 = @"
{'menu': {
  'id': 'file',
  'value': 'File',
  'popup': {
    'menuitem': [
      {'value': 'New', 'onclick': 'CreateNewDoc()'},
      {'value': 'Open', 'onclick': 'OpenDoc()'},
      {'value': 'Close', 'onclick': 'CloseDoc()'}
    ]
  }
}}

";

            string code3 = @"
  {'widget': 
  {
    'debug': 'on',
    'window': {
        'title': 'Sample Konfabulator Widget',        
        'name': 'main_window',        
        'width': 500,        
        'height': 500
            },   
    'image': { 
        'src': 'Images/Sun.png',
        'name': 'sun1',        
        'hOffset': 250,        
        'vOffset': 250,        
        'alignment': 'center'
        },  
    'text': {
        'data': 'Click Here',
        'size': 36,
        'style': 'bold',        
        'name': 'text1',        
        'hOffset': 250,        
        'vOffset': 100,        
        'alignment': 'center',
        'onMouseUp': 'sun1.opacity = (sun1.opacity / 100) * 90;'
    }
}}    

";

            string code4 = @"

{'web-app': {
  'servlet': [   
    {
      'servlet-name': 'cofaxCDS',
      'servlet-class': 'org.cofax.cds.CDSServlet',
      'init-param': {
        'configGlossary:installationAt': 'Philadelphia, PA',
        'configGlossary:adminEmail': 'ksm@pobox.com',
        'configGlossary:poweredBy': 'Cofax',
        'configGlossary:poweredByIcon': '/images/cofax.gif',
        'configGlossary:staticPath': '/content/static',
        'templateProcessorClass': 'org.cofax.WysiwygTemplate',
        'templateLoaderClass': 'org.cofax.FilesTemplateLoader',
        'templatePath': 'templates',
        'templateOverridePath': '',
        'defaultListTemplate': 'listTemplate.htm',
        'defaultFileTemplate': 'articleTemplate.htm',
        'useJSP': false,
        'jspListTemplate': 'listTemplate.jsp',
        'jspFileTemplate': 'articleTemplate.jsp',
        'cachePackageTagsTrack': 200,
        'cachePackageTagsStore': 200,
        'cachePackageTagsRefresh': 60,
        'cacheTemplatesTrack': 100,
        'cacheTemplatesStore': 50,
        'cacheTemplatesRefresh': 15,
        'cachePagesTrack': 200,
        'cachePagesStore': 100,
        'cachePagesRefresh': 10,
        'cachePagesDirtyRead': 10,
        'searchEngineListTemplate': 'forSearchEnginesList.htm',
        'searchEngineFileTemplate': 'forSearchEngines.htm',
        'searchEngineRobotsDb': 'WEB-INF/robots.db',
        'useDataStore': true,
        'dataStoreClass': 'org.cofax.SqlDataStore',
        'redirectionClass': 'org.cofax.SqlRedirection',
        'dataStoreName': 'cofax',
        'dataStoreDriver': 'com.microsoft.jdbc.sqlserver.SQLServerDriver',
        'dataStoreUrl': 'jdbc:microsoft:sqlserver://LOCALHOST:1433;DatabaseName=goon',
        'dataStoreUser': 'sa',
        'dataStorePassword': 'dataStoreTestQuery',
        'dataStoreTestQuery': 'SET NOCOUNT ON;select test=\'test\';',
        'dataStoreLogFile': '/usr/local/tomcat/logs/datastore.log',
        'dataStoreInitConns': 10,
        'dataStoreMaxConns': 100,
        'dataStoreConnUsageLimit': 100,
        'dataStoreLogLevel': 'debug',
        'maxUrlLength': 500}},
    {
      'servlet-name': 'cofaxEmail',
      'servlet-class': 'org.cofax.cds.EmailServlet',
      'init-param': {
      'mailHost': 'mail1',
      'mailHostOverride': 'mail2'}},
    {
      'servlet-name': 'cofaxAdmin',
      'servlet-class': 'org.cofax.cds.AdminServlet'},
 
    {
      'servlet-name': 'fileServlet',
      'servlet-class': 'org.cofax.cds.FileServlet'},
    {
      'servlet-name': 'cofaxTools',
      'servlet-class': 'org.cofax.cms.CofaxToolsServlet',
      'init-param': {
        'templatePath': 'toolstemplates/',
        'log': 1,
        'logLocation': '/usr/local/tomcat/logs/CofaxTools.log',
        'logMaxSize': '',
        'dataLog': 1,
        'dataLogLocation': '/usr/local/tomcat/logs/dataLog.log',
        'dataLogMaxSize': '',
        'removePageCache': '/content/admin/remove?cache=pages&id=',
        'removeTemplateCache': '/content/admin/remove?cache=templates&id=',
        'fileTransferFolder': '/usr/local/tomcat/webapps/content/fileTransferFolder',
        'lookInContext': 1,
        'adminGroupID': 4,
        'betaServer': true}}],
  'servlet-mapping': {
    'cofaxCDS': '/',
    'cofaxEmail': '/cofaxutil/aemail/*',
    'cofaxAdmin': '/admin/*',
    'fileServlet': '/static/*',
    'cofaxTools': '/tools/*'},
 
  'taglib': {
    'taglib-uri': 'cofax.tld',
    'taglib-location': '/WEB-INF/tlds/cofax.tld'}}}

";

            string code5 = @"
{'menu': {
    'header': 'SVG Viewer',
    'items': [
        {'id': 'Open'},
        {'id': 'OpenNew', 'label': 'Open New'},
        null,
        {'id': 'ZoomIn', 'label': 'Zoom In'},
        {'id': 'ZoomOut', 'label': 'Zoom Out'},
        {'id': 'OriginalView', 'label': 'Original View'},
        null,
        {'id': 'Quality'},
        {'id': 'Pause'},
        {'id': 'Mute'},
        null,
        {'id': 'Find', 'label': 'Find...'},
        {'id': 'FindAgain', 'label': 'Find Again'},
        {'id': 'Copy'},
        {'id': 'CopyAgain', 'label': 'Copy Again'},
        {'id': 'CopySVG', 'label': 'Copy SVG'},
        {'id': 'ViewSVG', 'label': 'View SVG'},
        {'id': 'ViewSource', 'label': 'View Source'},
        {'id': 'SaveAs', 'label': 'Save As'},
        null,
        {'id': 'Help'},
        {'id': 'About', 'label': 'About Adobe CVG Viewer...'}
    ]
}}


";

            string code6 = @"
{
Title :  'Input Calibration Due Date Range',
Parameters : {'Date1','Date2'},
Controls:
   [
     {  Class:'System.Windows.Forms.Label',   
        Text:'Cal Due Date From',   
        Width:200,
        Position:{1,1}   
     },
     {  
        Class: 'DevExpress.XtraEditors.DateEdit',
        Return: 'EditValue',
        EditValue: Date1.isnull(Date(2010,1,1)), 
        Name: 'Date1', 
        Position:{1,2}
     },
     {
       Class: 'System.Windows.Forms.Label',
       Text: 'Cal Due  Date To', 
       Position: {2,1}
     },
     {
       Class: 'DevExpress.XtraEditors.DateEdit',
       Return: 'EditValue',
       EditValue: Date2.isnull(Date(2010,1,1)),
       Name: 'Date2', 
       Position,{2,2}
     }
  ]

}
";
            VAL dict1 = Script.Evaluate(code1);
            VAL dict2 = Script.Evaluate(code2);
            VAL dict3 = Script.Evaluate(code3);
            VAL dict4 = Script.Evaluate(code4);
            VAL dict5 = Script.Evaluate(code5);
            VAL dict6 = Script.Evaluate(code6);
            System.Diagnostics.Debug.Assert(dict6["Controls"][1]["Return"].Str == "EditValue");

            string JSON1 = dict1.ToJson("");
            string JSON2 = dict2.ToJson("");
            string JSON3 = dict3.ToJson("");
            string JSON4 = dict4.ToJson("");
            string JSON5 = dict5.ToExJson();

            string code = @"
A={1,2,3,4,5,6};
B=A.slice(0,5,2);
text = 'OK';
    switch(text)
    {
    case 'OK': 
        Result = 1;
        break;
    case 'Cancel': 
        Result =2;
        break;
    }
";


            // Tie.Logger.Open("C:\\temp\\tie.log");
            Memory DS2 = new Memory();
            Script.Execute(code, DS2);


            VAL A = Script.Evaluate("{a:12, b:'ok', c:false, d:3.14}");
            var obj = Valizer.Devalize(A, new { a = 0, b = string.Empty, c = true });
            Debug.Assert(obj.a == 12 && obj.b == "ok" && obj.c == false);

            VAL B = Script.Evaluate("{ a:12, b:'ok', c:false, d:3.14, e: {a:1, b:3}}");
            var B1 = Valizer.Devalize(B, new { a = 0, b = string.Empty, c = true, e = new { a = 0, b = 0 } });
            Debug.Assert(B1.a==12 && B1.e.a == 1 && B1.e.b == 3);

            VAL B2 = Valizer.Valize(B1);
        }
    }
}
