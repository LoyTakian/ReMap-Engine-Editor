
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

using static Build.Build;

namespace Build
{
    public class BuildTextInfoPanel
    {
        public static async Task< StringBuilder > BuildTextInfoPanelObjects( GameObject[] objectData, BuildType buildType )
        {
            StringBuilder code = new StringBuilder();

            // Add something at the start of the text
            switch ( buildType )
            {
                case BuildType.Script:
                    code.Append( "    // Text Info Panels" );
                    PageBreak( ref code );
                    break;

                case BuildType.EntFile:
                    // Empty
                    break;

                case BuildType.Precache:
                    // Empty
                    break;

                case BuildType.DataTable:
                    // Empty
                    break;

                case BuildType.LiveMap:
                    // Empty
                break;
            }

            // Build the code
            foreach ( GameObject obj in objectData )
            {
                TextInfoPanelScript script = ( TextInfoPanelScript ) Helper.GetComponentByEnum( obj, ObjectType.TextInfoPanel );
                if ( script == null ) continue;

                switch ( buildType )
                {
                    case BuildType.Script:
                        code.Append( $"    MapEditor_CreateTextInfoPanel( \"{script.Title}\", \"{script.Description}\", {Helper.BuildOrigin( obj ) + Helper.ShouldAddStartingOrg()}, {Helper.BuildAngles( obj )}, {Helper.BoolToLower( script.showPIN )}, {Helper.ReplaceComma( script.Scale )})" + "\n" );
                        break;

                    case BuildType.EntFile:
                        // Empty
                        break;

                    case BuildType.Precache:
                        // Empty
                        break;

                    case BuildType.DataTable:
                        // Empty
                        break;

                    case BuildType.LiveMap:
                        CodeViewsWindow.LiveMap.SendCommandToApex($"script MapEditor_CreateTextInfoPanel( \"{script.Title}\", \"{script.Description}\", {Helper.BuildOrigin( obj, false, true )}, {Helper.BuildAngles( obj )}, {Helper.BoolToLower( script.showPIN )}, {Helper.ReplaceComma( script.Scale )})");
                        Helper.DelayInMS(CodeViewsWindow.LiveMap.BuildWaitMS);
                    break;
                }
            }

            // Add something at the end of the text
            switch ( buildType )
            {
                case BuildType.Script:
                    PageBreak( ref code );
                    break;

                case BuildType.EntFile:
                    // Empty
                    break;

                case BuildType.Precache:
                    // Empty
                    break;
                    
                case BuildType.DataTable:
                    // Empty
                    break;

                case BuildType.LiveMap:
                    // Empty
                break;
            }

            await Task.Delay( TimeSpan.FromSeconds( 0.001 ) );

            return code;
        }
    }
}
