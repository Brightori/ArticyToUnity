using Articy.Api;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MyCompany.TestArticy
{
    public class Parser
    {
        public void Process(ObjectProxy child)
        {
            var childs = child.GetChildren();
            
            foreach (var c in childs)
            {
                if (c.CanHaveChildren)
                {
                    var cc = c.GetChildren();
                    
                    foreach(var childi in cc)
                    {
                        GetProperties(childi);
                    }
                }

                GetProperties(c);
            }
        }

        private void GetProperties(ObjectProxy objectProxy)
        {
            var getProperties = objectProxy.GetAvailableProperties();
            var t = objectProxy.GetDisplayName();
            var templateName2 = objectProxy.GetTemplateTechnicalName();
            //var image = objectProxy.GetPreviewImage();
            //var imageId = image.GetDisplayName();
            var attachments = objectProxy.GetAttachments();
        }
    }
}
