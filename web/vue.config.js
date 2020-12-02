const path = require("path");
const vueSrc = "src";

//I noticed when we don't have publicPath set to /, it becomes problematic using vue-cli-tools - it doesn't seem to respect history paths. 
//When deployed over NGINX this problem seems to go away. So I've left it as / for now in local development environments. 
const webBaseHref = process.env.WEB_BASE_HREF || '/scjscv/';
module.exports = {
	publicPath: webBaseHref,
	configureWebpack: {
		devServer: {
			historyApiFallback: true,
			host: 'localhost',
			port: 1337,
			proxy: {
				//Development purposes, if WEB_BASE_HREF changes, this will have to change as well. 
				'^/scjscv/api': {
					target: "https://localhost:44369",
					pathRewrite: { '^/scjscv/api': '/api' },
					headers: {
						Connection: 'keep-alive',
						'X-Forwarded-Host': 'localhost',
						'X-Forwarded-Port': '1337'
					},
					changeOrigin: true
				},
				'^/api': {
					target: "https://localhost:44369",
					headers: {
						Connection: 'keep-alive',
						'X-Forwarded-Host': 'localhost',
						'X-Forwarded-Port': '1337'
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
