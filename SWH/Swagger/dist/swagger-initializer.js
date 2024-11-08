window.onload = function () {
    const ui = SwaggerUIBundle({
        url: "/swagger.json",  // URL to the OpenAPI spec
        dom_id: "#swagger-ui",
        deepLinking: true,
        presets: [
            SwaggerUIBundle.presets.apis,
            SwaggerUIStandalonePreset
        ],
        layout: "BaseLayout"
    });

    window.ui = ui;
};
