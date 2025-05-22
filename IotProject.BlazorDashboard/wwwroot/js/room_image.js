export async function UploadImage(uploadUrl) {
    const input = document.getElementById("room_image_input");
    const jwt = localStorage.getItem("JwtToken");

    if (input.files.length === 0) return;
    const file = input.files[0];

    // Create a FormData object and append the file
    const formData = new FormData();
    formData.append('file', file);

    try {
        const response = await fetch(uploadUrl, {
            method: 'POST',
            headers: {
                "Authorization": `Bearer ${jwt}`
            },
            body: formData,
        });

        // Handle errors based on response status
        if (!response.ok) {
            const errorMessage = await response.text();
            throw new Error(`Upload failed: ${errorMessage}.`);
        }

        // Convert the response
        const result = await response.text();
        console.log('Server response:', result);
    } catch (error) {
        console.error('Error uploading file:', error);
    };

    console.log(input.value);
    console.log(jwt);

    console.log(uploadUrl);
    console.log(file);



    //fetch(uploadUrl, {
    //    method: "POST",
    //    body: JSON
    //        ,
    //    headers: {
    //        "Content-type": "application/json",
    //        "Authorization": "bearer" + jwt
    //    }
    //})
    //    .then(response => response.json())
    //    .then(json => console.log(json));

    return input;
}