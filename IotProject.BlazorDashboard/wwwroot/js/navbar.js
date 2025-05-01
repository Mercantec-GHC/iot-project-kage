export function closeCollapse(id) {
    const bsCollapse = new bootstrap.Collapse(`#${id}`), {
        toggle: fasle
    });
    bsCollapse.hide();
}

export function openCollapse(id) {
    const bsCollapse = new bootstrap.Collapse(`#${id}`, {
        toggle: false
    });
    bsCollapse.show();
}