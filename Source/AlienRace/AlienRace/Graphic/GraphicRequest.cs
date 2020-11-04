using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace AvaliMod
{
    public struct AvaliGraphicRequest : IEquatable<AvaliGraphicRequest>
    {
        public System.Type graphicClass;
        public string path;
        public Shader shader;
        public Vector2 drawSize;
        public Color color;
        public Color colorTwo;
        public Color colorThree;
        public AvaliGraphicData graphicData;
        public int renderQueue;
        public List<ShaderParameter> shaderParameters;

        public AvaliGraphicRequest(
          System.Type graphicClass,
          string path,
          Shader shader,
          Vector2 drawSize,
          Color color,
          Color colorTwo,
          Color colorThree,
          AvaliGraphicData graphicData,
          int renderQueue,
          List<ShaderParameter> shaderParameters)
        {
            this.graphicClass = graphicClass;
            this.path = path;
            this.shader = shader;
            this.drawSize = drawSize;
            this.color = color;
            this.colorTwo = colorTwo;
            this.colorThree = colorThree;
            this.graphicData = graphicData;
            this.renderQueue = renderQueue;
            this.shaderParameters = shaderParameters.NullOrEmpty<ShaderParameter>() ? (List<ShaderParameter>)null : shaderParameters;
        }

        public override int GetHashCode()
        {
            if (this.path == null)
                this.path = BaseContent.BadTexPath;
            return Gen.HashCombine<List<ShaderParameter>>(Gen.HashCombine<int>(Gen.HashCombine<AvaliGraphicData>(Gen.HashCombineStruct<Color>(Gen.HashCombineStruct<Color>(Gen.HashCombineStruct<Vector2>(Gen.HashCombine<Shader>(Gen.HashCombine<string>(Gen.HashCombine<System.Type>(0, this.graphicClass), this.path), this.shader), this.drawSize), this.color), this.colorTwo), this.graphicData), this.renderQueue), this.shaderParameters);
        }

        public override bool Equals(object obj)
        {
            return obj is AvaliGraphicRequest other && this.Equals(other);
        }

        public bool Equals(AvaliGraphicRequest other)
        {
            return this.graphicClass == other.graphicClass && this.path == other.path && ((UnityEngine.Object)this.shader == (UnityEngine.Object)other.shader && this.drawSize == other.drawSize) && (this.color == other.color && this.colorTwo == other.colorTwo && (this.graphicData == other.graphicData && this.renderQueue == other.renderQueue)) && this.shaderParameters == other.shaderParameters;
        }

        public static bool operator ==(AvaliGraphicRequest lhs, AvaliGraphicRequest rhs)
        {
            return lhs.Equals(rhs);
        }

        public static bool operator !=(AvaliGraphicRequest lhs, AvaliGraphicRequest rhs)
        {
            return !(lhs == rhs);
        }
    }
}