window.addEventListener("load", function(){
	var code = document.getElementById("code");
	code.value = "";

	document.getElementById("exec").onclick = function(){
		ImageProcessing.load("barcode.png", function(self, e){
			code.value = self.readJAN();
		});
	};
}, false);
