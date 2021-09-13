using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SimpleSceneJumper.Editor
{
    public class SimpleSceneJumperScene
    {
        public string name;
        public string path;
        public bool scenesInBuild;
        public bool enabled;

        public SimpleSceneJumperScene(string path, bool scenesInBuild = false, bool enabled = false)
        {
            this.name = System.IO.Path.GetFileNameWithoutExtension(path);
            this.path = path;
            this.scenesInBuild = scenesInBuild;
            this.enabled = enabled;
        }

        public override string ToString()
        {
            return string.Format("name={0}, path={1}, scenesInBuild={2}, enabled={3}", name, path, scenesInBuild, enabled);
        }
    }
}
