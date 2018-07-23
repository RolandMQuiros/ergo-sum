using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ErgoSum.Utilities {
	[RequireComponent(typeof(Renderer))]
	public class UVAnimation : MonoBehaviour {
		public Vector2 Offset;
		private Renderer _renderer;
		private Vector4 _mainTexST;
		private void Awake() {
			_renderer = GetComponent<Renderer>();
			
			Vector2 texScale = _renderer.material.GetTextureScale("_MainTex");
			Vector2 texOffset = _renderer.material.GetTextureOffset("_MainTex");
			_mainTexST = new Vector4(texScale.x, texScale.y, texOffset.x, texOffset.y);
		}

		private void LateUpdate() {
			MaterialPropertyBlock block = new MaterialPropertyBlock();
			_mainTexST.z = Offset.x;
			_mainTexST.w = Offset.y;
			block.SetVector("_MainTex_ST", _mainTexST);
			_renderer.SetPropertyBlock(block);
		}
	}
}