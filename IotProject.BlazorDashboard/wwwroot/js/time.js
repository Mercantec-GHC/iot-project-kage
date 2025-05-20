export function convertUTCToLocal(utcDateString) {
    let utcDate = new Date(utcDateString);

    return utcDate.toLocaleString();
}

export function updateLastUpdateTime(localTime) {
    const timeElement = document.getElementById('lastUpdateTime');
    if (timeElement) {
        timeElement.textContent = localTime;
    }
}