const webpack = require('webpack')

module.exports = {
    publicPath: './',
	
	    //引入jquery
	    chainWebpack: config => {
	        config.plugin('provide').use(webpack.ProvidePlugin, [{
	            $: 'jquery',
	            jquery: 'jquery',
	            jQuery: 'jquery',
	            'window.jQuery': 'jquery'
	        }])
	    }
	 
}