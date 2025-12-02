function markdownLoad(mdText, id = 'markdown') {
    document.getElementById(id).innerHTML = marked.parse(mdText);
}