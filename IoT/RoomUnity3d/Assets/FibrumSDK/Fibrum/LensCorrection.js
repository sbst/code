
#pragma strict

@script ExecuteInEditMode
@script RequireComponent (Camera)
@script AddComponentMenu ("Image Effects/LensCorrection")

class LensCorrection extends PostEffectsBase {
	public var strengthX : float = 0.05f;
	public var strengthY : float = 0.05f;

	public var LensCorrectionShader : Shader = null;
	private var LensCorrectionMaterial : Material = null;	

	function DisableLensCorrection()
	{
		enabled = false; 
	}
   
	function CheckResources () : boolean {	
		CheckSupport (false);
		LensCorrectionMaterial = CheckShaderAndCreateMaterial(LensCorrectionShader, LensCorrectionMaterial);
		
		if(!isSupported)
			ReportAutoDisable ();
		return isSupported;			
	}
	
	function OnRenderImage (source : RenderTexture, destination : RenderTexture) {	

	
		if(CheckResources()==false) {
			Graphics.Blit (source, destination);
			return;
		}
				
		LensCorrectionMaterial.SetFloat ("k" , strengthX );
		LensCorrectionMaterial.SetFloat ("kcube" , strengthY );
		Graphics.Blit (source, destination, LensCorrectionMaterial); 	
	}
 
}