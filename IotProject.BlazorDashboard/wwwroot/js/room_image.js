export async function UploadImage(uploadUrl) {
    const input = document.getElementById("room_image_input");
    let jwt = localStorage.getItem("JwtToken");

    if (!jwt) {
        jwt = sessionStorage.getItem("JwtToken");
    }

    if (!jwt) return false;

    jwt = jwt.replaceAll("\"", "");

    if (input.files.length === 0) return false;
    const file = input.files[0];

    // Create a FormData object and append the file
    const formData = new FormData();
    formData.append('file', file);

    console.log(input.value);
    console.log(jwt);
    console.log(uploadUrl);
    console.log(file);

    try {
        const response = await fetch(uploadUrl, {
            method: 'POST',
            headers: {
                "Authorization": `bearer ${jwt}`
            },
            body: formData,
        });

        // Handle errors based on response status
        if (!response.ok) {
            const errorMessage = await response.text();
            console.log(`Upload failed: ${errorMessage}.`);

            return false;
        }

        // Convert the response
        const result = await response.text();
        console.log('Server response:', result);

        return true;
    } catch (error) {
        console.error('Error uploading file:', error);

        return false;
    };

    return input;
}