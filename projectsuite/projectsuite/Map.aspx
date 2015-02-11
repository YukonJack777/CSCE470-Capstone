<%@ Page Language="C#" AutoEventWireup="true" MasterPageFile="~/Main.master" CodeBehind="Map.aspx.cs" Inherits="projectsuite._Default" %>


<asp:Content ID="Content" ContentPlaceHolderID="MainContent" runat="server">
   <link rel="stylesheet" type="text/css" href="//js.arcgis.com/3.8/js/dojo/dijit/themes/claro/claro.css">
        <link rel="stylesheet" type="text/css" href="//js.arcgis.com/3.8/js/esri/css/esri.css" />
        <link rel="stylesheet" type="text/css" href="Content/agsjs.css" />
        <style>
            /*html, body {
                height: 99%;
                width: 99%;
                margin: 0;
                padding: 2px;
                font-family: helvetica, arial, sans-serif;
                font-size: 90%;
            }*/
            .maindiv{
                height: 727px;
            }
            .leftPane {
                width: 350px;
                overflow: auto
            }

            /* this line hide layers when out of scale for the inline TOC */
            /*.agsjsTOCOutOfScale {
                /*  display: none;*/
            /*}*/
        </style>
        <script type="text/javascript">
            // helpful for understanding dojoConfig.packages vs. dojoConfig.paths:
            // http://www.sitepen.com/blog/2013/06/20/dojo-faq-what-is-the-difference-packages-vs-paths-vs-aliases/
            var dojoConfig = {
                paths: {
                    //if you want to host on your own server, download and put in folders then use path like:
                    agsjs: location.pathname.replace(/\/[^/]+$/, '') + '/../Content'
                }
            };


       </script>
       <script src="//js.arcgis.com/3.8/"></script>

       <script type="text/javascript">
           var map, toc, dynaLayer1, dynaLayer2;
           require(["dojo/_base/connect",
            "dojo/dom", "dojo/parser", "dojo/on", "dojo/_base/Color",
            "esri/map",
            "esri/geometry/Extent",
            "esri/layers/FeatureLayer",
            "esri/layers/ArcGISTiledMapServiceLayer",
            "esri/layers/ArcGISDynamicMapServiceLayer",
            "esri/symbols/SimpleFillSymbol",
            "esri/renderers/ClassBreaksRenderer",
            "esri/dijit/BasemapGallery",
            "agsjs/dijit/TOC",
            "dijit/layout/BorderContainer",
            "dijit/layout/ContentPane",
            "dijit/TitlePane",
            "dojo/fx", "dojo/domReady!", "esri/arcgis/utils"], function (connect, dom, parser, on, Color,
            Map, Extent, FeatureLayer, ArcGISTiledMapServiceLayer, ArcGISDynamicMapServiceLayer,
            SimpleFillSymbol, ClassBreaksRenderer, BasemapGallery,
            TOC) {

                // call the parser to create the dijit layout dijits
                parser.parse(); // note djConfig.parseOnLoad = false;

                map = new Map(
                  "map", {
                      basemap: "topo",
                      center: [-154.116211, 62.021528],
                      zoom: 8
                  }
                );

                dynaLayer1 = new ArcGISDynamicMapServiceLayer("http://dnratwgisprod1.soa.alaska.gov:6080/arcgis/rest/services/Donlin/MapServer", {
                    opacity: 0.8
                });

                dynaLayer2 = new ArcGISDynamicMapServiceLayer("http://dnratwgisprod1.soa.alaska.gov:6080/arcgis/rest/services/DNRLandRecords/Ownership/MapServer", {
                    opacity: 0.8
                });

                map.on('layers-add-result', function (evt) {
                    // overwrite the default visibility of service.
                    // TOC will honor the overwritten value.
                    dynaLayer1.setVisibleLayers([2, 5, 8, 11]);
                    //dynaLayer2.setVisibleLayers([1]);
                    //try {

                    toc = new TOC(
                	{
                	    map: map,
                	    layerInfos: [
							{
							    layer: dynaLayer1,
							    title: "Donlin Project"
							    //collapsed: false, // whether this root layer should be collapsed initially, default false.
							    //slider: false // whether to display a transparency slider.
							},
							{
							    layer: dynaLayer2,
							    title: "Land Ownership"
							    //collapsed: false, // whether this root layer should be collapsed initially, default false.
							    //slider: false // whether to display a transparency slider.
							}
                	    ]
                	},
					'tocDiv');
                    toc.startup();
                    toc.on('load', function () {
                        if (console) console.log('TOC loaded');
                    });
                });

                map.addLayers([dynaLayer1, dynaLayer2]);


                /* on(dom.byId("SetVisibleLayersProgramatically"),'click', function(evt){
                dynaLayer1.setVisibleLayers([2, 17, 18, 19, 20]);
                  }); */

                on(dom.byId("FindNodeByLayer"), 'click', function (evt) {
                    // 0 is the layerId of group "Public Safety"
                    toc.findTOCNode(dynaLayer1, 0).collapse();
                    // 	12 is the id of layer "Damage Assessment"
                    toc.findTOCNode(dynaLayer1, 12).hide();
                });



                /* ------ */

                on(dom.byId("HandleNodeCheckEvent"), 'click', function (evt) {
                    toc.on('toc-node-checked', function (evt) {
                        // when check on one layer, turn off everything else on the public safety service.
                        if (evt.checked && evt.rootLayer && evt.serviceLayer && evt.rootLayer == dynaLayer1) {
                            evt.rootLayer.setVisibleLayers([evt.serviceLayer.id])
                        }
                        if (console) {
                            console.log("TOCNodeChecked, rootLayer:"
                            + (evt.rootLayer ? evt.rootLayer.id : 'NULL')
                            + ", serviceLayer:" + (evt.serviceLayer ? evt.serviceLayer.id : 'NULL')
                            + " Checked:" + evt.checked);
                        }
                    });
                });
                // end of example actions
            });

    	</script>

  
 <%--<body class="claro">--%>
    <div id="maindiv" >
        <div id="content" data-dojo-type="dijit/layout/BorderContainer" design="headline" gutters="true" style="width: 100%; height: 725px; margin: 0;">
            <%--<div id="header" data-dojo-type="dijit/layout/ContentPane" region="top" style="margin:0;">
            	<strong>Department of Natural Resources - Donlin Project Map</strong>
            </div>--%>
    
            <div data-dojo-type="dijit/layout/ContentPane" id="leftPane" region="left" splitter="true" style="width: 350px">
                <div id="tocDiv">
                </div>
            </div>
            <div id="map" data-dojo-type="dijit/layout/ContentPane" region="center">
            </div>

    </div>
        </div>
   <%-- </body>--%>


</asp:Content>