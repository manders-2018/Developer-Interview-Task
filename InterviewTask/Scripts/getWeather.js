document.addEventListener("DOMContentLoaded", function () {

    document.addEventListener("click", function () {

        if (event.target.classList.contains("fetchWeatherButton")) {
            const dataId = event.target.getAttribute("data-id");

            console.log(dataId);
            const partialArea = document.querySelector(`.partial-container[data-id="${dataId}"]`);

            console.log(partialArea);

            partialArea.style = "display:none";

            const baseUrl = window.location.origin;
            const endpoint = '/Home/GetWeatherResult';
            const url = new URL(endpoint, baseUrl);
            url.searchParams.append('serviceName', dataId);
            console.log(url);

            fetch(url, {
                method: 'GET',
            })
                .then(response => response.text())
                .then(html => {
                    partialArea.innerHTML = html;
                    partialArea.style = "";
                })
                .catch(error => {
                    console.error('Error updating partial:', error);
                    partialArea.innerHTML = '<p>Error loading partial.</p>';
                });
            }
        });

    });
