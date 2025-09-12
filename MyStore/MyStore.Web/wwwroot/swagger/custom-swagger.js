(() => {
    // Gắn sự kiện sau khi gọi API
    const oldFetch = window.fetch;
    window.fetch = async (...args) => {
        const response = await oldFetch(...args);

        // Nếu API là Login và thành công
        if (args[0].includes("/api/Login") && response.ok) {
            response.clone().json().then(data => {
                if (data.token) {
                    const token = data.token;

                    // ✅ Tự động set vào Swagger Authorize
                    const bearerToken = "Bearer " + token;
                    window.localStorage.setItem("jwt-token", bearerToken);

                    const ui = window.ui || window.swaggerUi;
                    if (ui) {
                        ui.preauthorizeApiKey("Bearer", bearerToken);
                        console.log("JWT saved and applied automatically:", bearerToken);
                    }
                }
            });
        }
        return response;
    };
})();
