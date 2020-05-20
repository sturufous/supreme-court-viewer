module.exports = {
	configureWebpack: {
		devServer: {
			historyApiFallback: true,
			proxy: {
				'^/api': {
					target: "https://localhost:44369"
				}
			}
		}
	}
};