document.querySelectorAll('.TimelineItem-body a').forEach(item => {
    item.addEventListener('click', event => {
        event.preventDefault();
        window.location = event.target.href;
        return false;
    })
})