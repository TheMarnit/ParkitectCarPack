using System;
using System.Collections.Generic;
using System.IO;
using Custom_Scenery.CustomScenery;
using Custom_Scenery.CustomScenery.Decorators;
using Custom_Scenery.Decorators;
using MiniJSON;
using UnityEngine;

namespace Custom_Scenery
{
    internal class SceneryLoader : MonoBehaviour
    {
        private List<BuildableObject> _sceneryObjects = new List<BuildableObject>();

        public string Path;
		
        public Dictionary<string, object> Settings;

        public string Identifier;

        public void LoadScenery()
        {
            try
            {
                var dict = Json.Deserialize(File.ReadAllText(Path + @"/scenery.json")) as Dictionary<string, object>;
				GameObject hider = new GameObject();

                char dsc = System.IO.Path.DirectorySeparatorChar;

                using (WWW www = new WWW("file://" + Path + dsc + "assetbundle" + dsc + "scenery"))
                {
                    if (www.error != null)
                        throw new Exception("Loading had an error:" + www.error);

                    AssetBundle bundle = www.assetBundle;

                    foreach (KeyValuePair<string, object> pair in dict)
                    {
                        try
                        {
							var options = pair.Value as Dictionary<string, object>;
							if((bool)Settings["enableFlippedReliant"]==true||!options.ContainsKey("isFlippedReliant")) {
								string mycategory = "";
								if((bool)Settings["carPackGroup"]==true) {
									mycategory += "Cars 'R' Us";
								}
								if((bool)Settings["seperateClass"]==true) {
									if((string)options["class"]=="V") {
										mycategory += (mycategory!=""?"/":"")+"Vehicles";
									}
								}
								if((bool)Settings["seperateGroup"]==true) {
									if((string)options["group"]=="C") {
										mycategory += (mycategory!=""?"/":"")+"Cars";
									}
								}
								if((bool)Settings["seperateBrand"]==true) {
									if((string)options["brand"]=="CI") {
										mycategory += (mycategory!=""?"/":"")+"Citroën";
									}
									if((string)options["brand"]=="VW") {
										mycategory += (mycategory!=""?"/":"")+"Volkswagen";
									}
									if((string)options["brand"]=="TS") {
										mycategory += (mycategory!=""?"/":"")+"Tesla";
									}
									if((string)options["brand"]=="CH") {
										mycategory += (mycategory!=""?"/":"")+"Chevrolet";
									}
									if((string)options["brand"]=="RL") {
										mycategory += (mycategory!=""?"/":"")+"Reliant";
									}
								}
								if((bool)Settings["seperateCarType"]==true) {
									if((string)options["cartype"]=="ST") {
										mycategory += (mycategory!=""?"/":"")+"Station Wagon";
									}
									if((string)options["cartype"]=="SD") {
										mycategory += (mycategory!=""?"/":"")+"Sedan";
									}
									if((string)options["cartype"]=="EX") {
										mycategory += (mycategory!=""?"/":"")+"Executive";
									}
									if((string)options["cartype"]=="PC") {
										mycategory += (mycategory!=""?"/":"")+"Pony Car";
									}
									if((string)options["cartype"]=="CC") {
										mycategory += (mycategory!=""?"/":"")+"City Car";
									}
								}
								GameObject asset = (new TypeDecorator((string)options["type"])).Decorate(options, bundle);
								asset.GetComponent<BuildableObject>().categoryTag = mycategory;
								(new PriceDecorator((double)options["price"])).Decorate(asset, options, bundle);
								(new NameDecorator(pair.Key)).Decorate(asset, options, bundle);

								if (options.ContainsKey("grid"))
									(new GridDecorator((bool)options["grid"])).Decorate(asset, options, bundle);
								
								if (options.ContainsKey("recolorable"))
									(new RecolorableDecorator((bool)options["recolorable"])).Decorate(asset, options, bundle);

								DontDestroyOnLoad(asset);

								AssetManager.Instance.registerObject(asset.GetComponent<BuildableObject>());
								_sceneryObjects.Add(asset.GetComponent<BuildableObject>());
								
								// hide it from view
								asset.transform.parent = hider.transform;
							}
						}
                        catch (Exception e)
                        {
                            Debug.Log(e);

                            LogException(e);
                        }
                    }

                    bundle.Unload(false);
                }

                hider.SetActive(false);
            }
            catch(Exception e)
            {
                LogException(e);
            }
        }

        private void LogException(Exception e)
        {
            StreamWriter sw = File.AppendText(Path + @"/mod.log");

            sw.WriteLine(e);

            sw.Flush();

            sw.Close();
        }

        public void UnloadScenery()
        {
            foreach (BuildableObject deco in _sceneryObjects)
            {
                AssetManager.Instance.unregisterObject(deco);
            }
        }
    }
}
