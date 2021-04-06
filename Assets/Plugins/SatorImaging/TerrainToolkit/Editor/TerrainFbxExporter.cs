namespace SatorImaging.TerrainToolkit
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEditor;
    using System.Text;
    using System.IO;
    using System;


    public static class TerrainFbxExporter
    {
        private const string MenuItemName = "Export Terrain as FBX...";


        static Terrain terrain;
        static float progress = 0f;


        //[MenuItem("File/Export Terrain...", priority = 9999)]
        //[MenuItem("Edit/" + MenuItemName, priority = 9999)]
        //[MenuItem("Assets/" + MenuItemName, priority = 9999)]
        [MenuItem("GameObject/" + MenuItemName, priority = 39)]
        [MenuItem("Component/" + MenuItemName, priority = 9999)]
        public static void Export()
        {
            progress = 0f;

            var sel = Selection.activeGameObject;
            if (!sel)
            {
                EditorUtility.DisplayDialog(typeof(TerrainFbxExporter).Namespace, "No Terrain selected.", "OK");
                return;
            }

            terrain = sel.GetComponent<Terrain>();
            if (!terrain)
            {
                EditorUtility.DisplayDialog(typeof(TerrainFbxExporter).Namespace, "Selected object is NOT Terrain.",
                    "OK");
                return;
            }


            float scale_factor = 1f;

            var mesh_name = "UnityTerrain_" + sel.gameObject.name;
            var mat_name = "UnityTerrain_Material";

            // export path
            var fbx_path = EditorUtility.SaveFilePanel(typeof(TerrainFbxExporter).Namespace, null, mesh_name, "fbx");
            if (string.IsNullOrEmpty(fbx_path))
            {
                return;
            }


            using (var sw = new StreamWriter(fbx_path))
            {
                var sb = new StringBuilder();
                // HEADER //////////////////////////////////////////////////
                sb.Append(@"; FBX 6.1.0 project file
; Copyright (C) 1997-2010 Autodesk Inc. and/or its licensors.
; All rights reserved.
; ----------------------------------------------------

FBXHeaderExtension:  {
    FBXHeaderVersion: 1003
    FBXVersion: 6100
    CurrentCameraResolution:  {
        CameraName: ""Producer Perspective""
        CameraResolutionMode: ""Fixed Resolution""
        CameraResolutionW: 960
        CameraResolutionH: 540
    }
    CreationTimeStamp:  {
        Version: 1000
        Year: " + DateTime.Now.Year + @"
        Month: " + DateTime.Now.Month + @"
        Day: " + DateTime.Now.Day + @"
        Hour: " + DateTime.Now.Hour + @"
        Minute: " + DateTime.Now.Minute + @"
        Second: " + DateTime.Now.Second + @"
        Millisecond: " + DateTime.Now.Millisecond + @"
    }
    Creator: """ + typeof(TerrainFbxExporter).FullName + @"""
}

; Document Description
;------------------------------------------------------------------

Document:  {
    Name: """ + mesh_name + @"""
}

; Document References
;------------------------------------------------------------------

References:  {
}

; Object definitions
;------------------------------------------------------------------

Definitions:  {
    Version: 100
    Count: 4
    ObjectType: ""Model"" {
        Count: 1
    }
    ObjectType: ""Material"" {
        Count: 1
    }
    ObjectType: ""SceneInfo"" {
        Count: 1
    }
    ObjectType: ""GlobalSettings"" {
        Count: 1
    }
}

; Object properties
;------------------------------------------------------------------

Objects:  {
    Model: ""Model::" + mesh_name + @""", ""Mesh"" {
        Version: 232
        Properties60:  {
            Property: ""GeometricTranslation"", ""Vector3D"", """",0,0,0
            Property: ""GeometricRotation"", ""Vector3D"", """",0,0,0
            Property: ""GeometricScaling"", ""Vector3D"", """",1,1,1
            Property: ""Show"", ""bool"", """",1
            Property: ""NegativePercentShapeSupport"", ""bool"", """",1
            Property: ""DefaultAttributeIndex"", ""int"", """",0
            Property: ""Lcl Translation"", ""Lcl Translation"", ""A""," + ((double) -terrain.transform.position.x) +
                          "," + ((double) terrain.transform.position.y) + "," +
                          ((double) terrain.transform.position.z) + @"
            Property: ""Lcl Rotation"", ""Lcl Rotation"", ""A"",0,0,0
            Property: ""Lcl Scaling"", ""Lcl Scaling"", ""A"",1,1,1
            Property: ""Visibility"", ""Visibility"", ""A"",1
            Property: ""currentUVSet"", ""KString"", ""U"", ""map1""
            Property: ""Color"", ""ColorRGB"", ""N"",0.8,0.8,0.8
            Property: ""BBoxMin"", ""Vector3D"", ""N"",0,0,0
            Property: ""BBoxMax"", ""Vector3D"", ""N"",0,0,0
            Property: ""Primary Visibility"", ""bool"", ""N"",1
            Property: ""Casts Shadows"", ""bool"", ""N"",1
            Property: ""Receive Shadows"", ""bool"", ""N"",1
        }
        MultiLayer: 0
        MultiTake: 0
        Shading: T
        Culling: ""CullingOff""

");


                sw.Write(sb.ToString());
                sb.Length = 0; // clear

                ShowProgressBar("Exporting Vertices");


                // VERTEX LOOP //////////////////////////////////////////////////
                sb.Append("        Vertices: ");

                // write once


                //Debug.LogFormat("TerrainData.Size: {0}", terrain.terrainData.size);
                //Debug.LogFormat("TerrainData.Heightmap Width x Height: {0} x {1}", terrain.terrainData.heightmapWidth, terrain.terrainData.heightmapHeight);
                //Debug.LogFormat("TerrainData.Heightmap Resolution: {0}", terrain.terrainData.heightmapResolution);

                float scale_x = scale_factor *
                                (terrain.terrainData.size.x / (terrain.terrainData.heightmapResolution - 1));
                //float scale_x = scale_factor * terrain.terrainData.size.x;
                //scale_x *= terrain.terrainData.size.x;

                //float scale_y = terrain.terrainData.heightmapHeight;
                float scale_y = scale_factor * terrain.terrainData.size.y;
                scale_y = scale_factor;

                float scale_z = scale_factor *
                                (terrain.terrainData.size.z / (terrain.terrainData.heightmapResolution - 1));
                //float scale_z = scale_factor * terrain.terrainData.size.z;
                //scale_z *= terrain.terrainData.size.z;

                float pos_y = 0;
                for (var z = 0; z < terrain.terrainData.heightmapResolution; z++)
                {
                    for (var x = 0; x < terrain.terrainData.heightmapResolution; x++)
                    {
                        pos_y = terrain.terrainData.GetHeight(x, z);

                        sb.Append(Convert.ToString((double) (-x * scale_x)).Replace(',', '.') + ',');
                        sb.Append(Convert.ToString((double) (pos_y * scale_y)).Replace(',', '.') + ',');
                        sb.Append(Convert.ToString((double) (z * scale_z)).Replace(',', '.') + ',');
                    }
                }


                sw.WriteLine(sb.ToString());
                sb.Length = 0; // clear

                ShowProgressBar("Exporting Polygons");


                // POLYGON LOOP //////////////////////////////////////////////////
                sb.Append("        PolygonVertexIndex: ");
                int num_polygons = (terrain.terrainData.heightmapResolution - 1) *
                                   (terrain.terrainData.heightmapResolution);
                int index_offset = terrain.terrainData.heightmapResolution;
                int skip_index = terrain.terrainData.heightmapResolution;

                for (var i = 0; i < num_polygons; i++)
                {
                    if (0 == (i + 1) % terrain.terrainData.heightmapResolution)
                    {
                        continue;
                    }

                    // v0, v1, v2, v3 will be... 0,1,2,-4
                    sb.Append(i + "," + (i + 1) + "," + (index_offset + i + 1) + ",-" + (index_offset + i + 1) + ",");
                }


                sw.WriteLine(sb.ToString());
                sb.Length = 0; // clear

                ShowProgressBar("Exporting Edges");


                // EDGE LOOP //////////////////////////////////////////////////
                //sw.WriteLine("        Edges: ");
                //sw.WriteLine ("");


                sw.WriteLine("        GeometryVersion: 124");

                ShowProgressBar("Exporting Normals");


                // NORMALS ///////////////////////////////////////////////////
                sb.Append(@"
        LayerElementNormal: 0 {
            Version: 101
            Name: """"
            MappingInformationType: ""ByVertice""
            ReferenceInformationType: ""Direct""
            Normals: ");

                float local_pos_x = 1f / (terrain.terrainData.heightmapResolution - 1);
                float local_pos_z = 1f / (terrain.terrainData.heightmapResolution - 1);

                var norm = new Vector3();
                for (var z = 0; z < terrain.terrainData.heightmapResolution; z++)
                {
                    for (var x = 0; x < terrain.terrainData.heightmapResolution; x++)
                    {
                        //norm = terrain.terrainData.GetInterpolatedNormal((x % (terrain.terrainData.heightmapWidth - 1)) * local_pos_x, z * local_pos_z);
                        norm = terrain.terrainData.GetInterpolatedNormal(x * local_pos_x, z * local_pos_z);

                        sb.Append(-norm.x + "," + norm.y + "," + norm.z + ",");
                    }
                }

                sb.Append(@"
        }");


                sw.WriteLine(sb.ToString());
                sb.Length = 0; // clear

                ShowProgressBar("Exporting UVs");


                // UV ////////////////////////////////////////////////////////
                sb.Append(@"
        LayerElementUV: 0 {
            Version: 101
            Name: ""map1""
            MappingInformationType: ""ByPolygonVertex""
            ReferenceInformationType: ""IndexToDirect""
            UV: ");

                local_pos_x = 1f / (terrain.terrainData.heightmapResolution - 1);
                local_pos_z = 1f / (terrain.terrainData.heightmapResolution - 1);

                for (var z = 0; z < terrain.terrainData.heightmapResolution; z++)
                {
                    for (var x = 0; x < terrain.terrainData.heightmapResolution; x++)
                    {
                        sb.Append((x * local_pos_x) + "," + (z * local_pos_z) + ",");
                    }
                }

                sb.AppendLine("");


                // UV INDEX ////////////////////////////////////////////////////////
                // tired...
                sb.Append(@"
            UVIndex: ");
                for (var i = 0; i < num_polygons; i++)
                {
                    if (0 == (i + 1) % terrain.terrainData.heightmapResolution)
                    {
                        continue;
                    }

                    // v0, v1, v2, v3 will be... 0,1,2,-4
                    sb.Append(i + "," + (i + 1) + "," + (i + 1 + index_offset) + "," + (i + 0 + index_offset) + ",");
                }

                sb.Append(@"
        }");


                sw.WriteLine(sb.ToString());
                sb.Length = 0; // clear

                ShowProgressBar("Building FBX Scene");


                sb.Append(@"
        LayerElementMaterial: 0 {
            Version: 101
            Name: """"
            MappingInformationType: ""AllSame""
            ReferenceInformationType: ""IndexToDirect""
            Materials: 0
        }
        Layer: 0 {
            Version: 100
            LayerElement:  {
                Type: ""LayerElementNormal""
                TypedIndex: 0
            }
            LayerElement:  {
                Type: ""LayerElementMaterial""
                TypedIndex: 0
            }
            LayerElement:  {
                Type: ""LayerElementColor""
                TypedIndex: 0
            }
            LayerElement:  {
                Type: ""LayerElementUV""
                TypedIndex: 0
            }
        }
        NodeAttributeName: ""Geometry::" + mesh_name + @"_ncl1_1""
    }");


                sb.Append(@"
    SceneInfo: ""SceneInfo::GlobalInfo"", ""UserData"" {
        Type: ""UserData""
        Version: 100
        MetaData:  {
            Version: 100
            Title: """"
            Subject: """"
            Author: """"
            Keywords: """"
            Revision: """"
            Comment: """"
        }
        Properties60:  {
        }
    }
    Material: ""Material::" + mat_name + @""", """" {
        Version: 102
        ShadingModel: ""lambert""
        MultiLayer: 0
        Properties60:  {
            Property: ""ShadingModel"", ""KString"", """", ""Lambert""
            Property: ""MultiLayer"", ""bool"", """",0
            Property: ""EmissiveColor"", ""Color"", ""A"",0,0,0
            Property: ""EmissiveFactor"", ""Number"", ""A"",1
            Property: ""AmbientColor"", ""Color"", ""A"",0,0,0
            Property: ""AmbientFactor"", ""Number"", ""A"",1
            Property: ""DiffuseColor"", ""Color"", ""A"",0.5,0.5,0.5
            Property: ""DiffuseFactor"", ""Number"", ""A"",0.8
            Property: ""Bump"", ""Vector3D"", """",0,0,0
            Property: ""NormalMap"", ""Vector3D"", """",0,0,0
            Property: ""BumpFactor"", ""double"", """",1
            Property: ""TransparentColor"", ""Color"", ""A"",0,0,0
            Property: ""TransparencyFactor"", ""Number"", ""A"",1
            Property: ""DisplacementColor"", ""ColorRGB"", """",0,0,0
            Property: ""DisplacementFactor"", ""double"", """",1
            Property: ""VectorDisplacementColor"", ""ColorRGB"", """",0,0,0
            Property: ""VectorDisplacementFactor"", ""double"", """",1
            Property: ""Emissive"", ""Vector3D"", """",0,0,0
            Property: ""Ambient"", ""Vector3D"", """",0,0,0
            Property: ""Diffuse"", ""Vector3D"", """",0.4,0.4,0.4
            Property: ""Opacity"", ""double"", """",1
        }
    }
    GlobalSettings:  {
        Version: 1000
        Properties60:  {
            Property: ""UpAxis"", ""int"", """",1
            Property: ""UpAxisSign"", ""int"", """",1
            Property: ""FrontAxis"", ""int"", """",2
            Property: ""FrontAxisSign"", ""int"", """",1
            Property: ""CoordAxis"", ""int"", """",0
            Property: ""CoordAxisSign"", ""int"", """",1
            Property: ""OriginalUpAxis"", ""int"", """",1
            Property: ""OriginalUpAxisSign"", ""int"", """",1
            Property: ""UnitScaleFactor"", ""double"", """",100
            Property: ""OriginalUnitScaleFactor"", ""double"", """",1
            Property: ""AmbientColor"", ""ColorRGB"", """",0,0,0
            Property: ""DefaultCamera"", ""KString"", """", ""Producer Perspective""
            Property: ""TimeMode"", ""enum"", """",11
            Property: ""TimeProtocol"", ""enum"", """",2
            Property: ""SnapOnFrameMode"", ""enum"", """",0
            Property: ""TimeSpanStart"", ""KTime"", """",1924423250
            Property: ""TimeSpanStop"", ""KTime"", """",384884650000
            Property: ""CustomFrameRate"", ""double"", """",-1
        }
    }
}
");


                // CONNECTION //////////////////////////////////////////////////
                sb.Append(@"
; Object connections
;------------------------------------------------------------------

Connections:  {
        Connect: ""OO"", ""Model::" + mesh_name + @""", ""Model::Scene""
        Connect: ""OO"", ""Material::" + mat_name + @""", ""Model::" + mesh_name + @"""
}

;Takes and animation section
;----------------------------------------------------

Takes:  {
    Current: ""Take 001""
    Take: ""Take 001"" {
        FileName: ""Take_001.tak""
        LocalTime: 1924423250,230930790000
        ReferenceTime: 1924423250,230930790000

        ;Models animation
        ;----------------------------------------------------

        ;Generic nodes animation
        ;----------------------------------------------------

        ;Textures animation
        ;----------------------------------------------------

        ;Materials animation
        ;----------------------------------------------------

        ;Constraints animation
        ;----------------------------------------------------
    }
}

;Version 5 settings
;------------------------------------------------------------------

Version5:  {
    AmbientRenderSettings:  {
        Version: 101
        AmbientLightColor: 0,0,0,1
    }
    FogOptions:  {
        FlogEnable: 0
        FogMode: 0
        FogDensity: 0.002
        FogStart: 0.3
        FogEnd: 1000
        FogColor: 1,1,1,1
    }
    Settings:  {
        FrameRate: ""24""
        TimeFormat: 1
        SnapOnFrames: 0
        ReferenceTimeIndex: -1
        TimeLineStartTime: 1924423250
        TimeLineStopTime: 384884650000
    }
    RendererSetting:  {
        DefaultCamera: ""Producer Perspective""
        DefaultViewingMode: 0
    }
}");


                sw.WriteLine(sb.ToString());
                sb.Length = 0; // clear


                EditorUtility.ClearProgressBar();
                sw.Close();
                sw.Dispose();


                Debug.Log(".fbx Exported successfully.");
                AssetDatabase.Refresh();
            }
        }


        static void ShowProgressBar(string info)
        {
            progress += 1f / (6f);

            EditorUtility.DisplayProgressBar(typeof(TerrainFbxExporter).Namespace, info, progress);
        }
    }
}