export function getBootstrapBreakpoint() {
    const width = window.innerWidth;
    if (width < 576) {
        return 'xs';
    } else if (width < 768) {
        return 'sm';
    } else if (width < 992) {
        return 'md';
    } else if (width < 1200) {
        return 'lg';
    } else if (width < 1400) {
        return 'xl';
    } else {
        return 'xxl';
    }
}

export function onResize(dotnetHelper) {
    window.addEventListener('resize', () => {
        const breakpoint = getBootstrapBreakpoint();
        dotnetHelper.invokeMethodAsync('OnResize', breakpoint);
    });
}