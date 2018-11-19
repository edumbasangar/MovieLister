function throwOnError(response) {
    if (!response.ok) {
        return response.json().then(err => { throw err; });
    }
    return response;
}

export default throwOnError;
