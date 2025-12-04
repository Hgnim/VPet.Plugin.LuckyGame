function markdownLoad(mdText, id = 'markdown') {
    const container = document.getElementById(id);
    container.innerHTML = marked.parse(mdText);

    // 为每个代码块注入行号并包裹 pre，以便显示行号列
    const codes = container.querySelectorAll('pre > code');
    codes.forEach(code => {
        const pre = code.parentElement;
        if (!pre || pre.classList.contains('lned')) return; // 已处理过

        // 计算行数（保留最后一行可能为空的情况）
        const text = code.textContent.replace(/\r\n/g, '\n');
        const lines = text.split('\n');

        // 创建行号列
        const gutter = document.createElement('div');
        gutter.className = 'gutter';
        gutter.setAttribute('aria-hidden', 'true');
        gutter.innerHTML = lines.map((_, i) => `<span class="line">${i + 1}</span>`).join('');

        // 用 wrapper 包裹 pre 和 gutter
        const wrapper = document.createElement('div');
        wrapper.className = 'code-with-line-numbers';

        pre.parentNode.insertBefore(wrapper, pre);
        wrapper.appendChild(gutter);
        wrapper.appendChild(pre);

        pre.classList.add('lned');
    });
}

// 如果页面上存在全局 markdownText，自动渲染（兼容原有页面直接调用 markdownLoad(markdownText)）
if (typeof markdownText !== 'undefined' && document.readyState === 'loading') {
    document.addEventListener('DOMContentLoaded', () => markdownLoad(markdownText));
}