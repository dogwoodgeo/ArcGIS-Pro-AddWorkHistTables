using System;
using System.Collections.Generic;
using ArcGIS.Core.CIM;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Dialogs;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;

namespace AddWorkHistTables
{
    internal class WorkHistTables : Button
    {
        protected override void OnClick()
        {
            QueuedTask.Run(() =>
            {
                try
                {
                    var mapView = MapView.Active.Map;
                    string urlMH = @"O:\SHARE\405 - INFORMATION SERVICES\GIS_Layers\GISVIEWER.SDE@SQL0.sde\SDE.sewerman.tblV8MHWorkhist";
                    string urlLines = @"O:\SHARE\405 - INFORMATION SERVICES\GIS_Layers\GISVIEWER.SDE@SQL0.sde\SDE.sewerman.tblV8SewerWorkHist";
                    Uri mhURI = new Uri(urlMH);
                    Uri linesURI = new Uri(urlLines);

                    // Check to see if the tables are already added to the map.
                    IReadOnlyList<StandaloneTable> mhTables = mapView.FindStandaloneTables("Manholes Work History");
                    IReadOnlyList<StandaloneTable> slTables = mapView.FindStandaloneTables("Sewer Lines Work History");
                    {
                        if (mhTables.Count == 0 && slTables.Count == 0)
                        {
                            StandaloneTable manholesHistory = StandaloneTableFactory.Instance.CreateStandaloneTable(mhURI, mapView, "Manholes Work History");
                            SetDisplayField(manholesHistory, "Manholes Work History", "COMPDTTM");
                            StandaloneTable linesHistory = StandaloneTableFactory.Instance.CreateStandaloneTable(linesURI, mapView, "Sewer Lines Work History");
                            SetDisplayField(linesHistory, "Sewer Lines Work History", "COMPDTTM");
                        }
                        else if (mhTables.Count > 0 && slTables.Count == 0)
                        {
                            StandaloneTable linesHistory = StandaloneTableFactory.Instance.CreateStandaloneTable(linesURI, mapView, "Sewer Lines Work History");
                            SetDisplayField(linesHistory, "Sewer Lines Work History", "COMPDTTM");
                            MessageBox.Show("'Manholes Work History' table is already present in map.\n\n'Sewer Lines Work History' table has been added", "Warning");
                        }

                        else if (mhTables.Count == 0 && slTables.Count > 0)
                        {
                            StandaloneTable manholesHistory = StandaloneTableFactory.Instance.CreateStandaloneTable(mhURI, mapView, "Manholes Work History");
                            SetDisplayField(manholesHistory, "Manholes Work History", "COMPDTTM");
                            MessageBox.Show("'Sewer Lines Work History' table is already present in map.\n\n'Manholes Work History' table has been added", "Warning");
                        }

                        else if (mhTables.Count > 0 && slTables.Count > 0)
                        {
                            MessageBox.Show("Sewer Lines and Manholes Work History tables are already present in map.", "Warning");
                        }
                    }
                }
                catch (Exception ex)
                {
                    string caption = "Failed to add work history tables!";
                    string message = "Process failed. \n\nSave and restart ArcGIS Pro and try process again.\n\n" +
                        $"If problem persist, contact your local GIS nerd.\n\n{ex}";

                    //Using the ArcGIS Pro SDK MessageBox class
                    MessageBox.Show(message, caption);
                }
            });
        }


        // Method to set the DisplayField property.
        public void SetDisplayField(StandaloneTable saTable, string tableName, string fieldName)
        {
            var mapView = MapView.Active.Map;

            QueuedTask.Run(() =>
            {
                //Gets a list of the standalone tables in map with the name specified in the method arguments.
                IReadOnlyList<StandaloneTable> tables = mapView.FindStandaloneTables(tableName);
                foreach (var table in tables)
                {
                    //Gets the list of fields from the stand alone table.
                    var descriptions = table.GetFieldDescriptions(); 
                    foreach (var desc in descriptions)
                    {
                        if (desc.Name == fieldName) // If field name equals "COMPDTTM"
                        {
                            //Creates variable that's equal to "COMPDTTM"
                            string displayName = desc.Name;

                            // Get's the CIM definition from the StandaloneTable
                            var cimTableDefinition = table.GetDefinition() as CIMStandaloneTable;

                            // Set DisplayField property of the cimTableDefinition equall to displayName("COMPDTTM")
                            cimTableDefinition.DisplayField = displayName;
                    
                            // USe the SetDefinition method to apply the modified table definition (cimTableDefninition)
                            saTable.SetDefinition(cimTableDefinition);

                            // Use to check the result when debugin in break point.
                            var result = table.GetDefinition().DisplayField;
                        }
                    }
                }
            });
        }
    }
}
