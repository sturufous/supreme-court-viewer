const path = require("path");
const vueSrc = "src";
module.exports = {
	configureWebpack: {
		devServer: {
			historyApiFallback: true,
			proxy: {
				'^/api': {
					target: "https://localhost:44369",
					headers: {
						Connection: 'keep-alive'
					},
					changeOrigin: true
				}
			}
		},
		resolve: {
			modules: [vueSrc],
			alias: {
				"@": path.resolve(__dirname, vueSrc),
				"@assets": path.resolve(__dirname, vueSrc.concat("/assets")),
				"@components": path.resolve(__dirname, vueSrc.concat("/components")),
				"@router": path.resolve(__dirname, vueSrc.concat("/router")),
				"@store": path.resolve(__dirname, vueSrc.concat("/store")),
				"@styles": path.resolve(__dirname, vueSrc.concat("/styles"))
			},
			extensions: ['.ts', '.vue', '.json', '.scss', '.svg', '.png', '.jpg']
		}
	}
};