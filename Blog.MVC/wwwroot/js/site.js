(function () {
    const input = document.getElementById('site-search-input');
    if (!input || typeof autoComplete === 'undefined') {
        return;
    }

    const searchUrl = input.dataset.searchUrl;
    const detailPath = input.dataset.detailPath;
    if (!searchUrl || !detailPath) {
        return;
    }

    new autoComplete({
        selector: () => input,
        debounce: 250,
        data: {
            src: async (query) => {
                const keyword = query.trim();
                if (keyword.length < 1) {
                    return [];
                }

                const response = await fetch(`${searchUrl}?q=${encodeURIComponent(keyword)}`);
                if (!response.ok) {
                    return [];
                }

                return response.json();
            },
            keys: ['title'],
            cache: false
        },
        resultsList: {
            maxResults: 8,
            noResults: true,
            tabSelect: true,
            class: 'site-search-dropdown'
        },
        resultItem: {
            highlight: true,
            element: (item, result) => {
                const article = result.value;
                const titleHtml = result.match;
                const categoryName = article.categoryName ?? article.CategoryName ?? '';
                const category = categoryName
                    ? `<span class="site-search-suggest-meta">${categoryName}</span>`
                    : '';
                item.innerHTML = `<span class="site-search-suggest-title">${titleHtml}</span>${category}`;
            }
        },
        events: {
            input: {
                selection(event) {
                    const article = event.detail.selection.value;
                    const slug = article?.slug ?? article?.Slug;
                    if (slug) {
                        window.location.href = `${detailPath}${encodeURIComponent(slug)}`;
                    }
                }
            }
        },
        submit: true
    });
})();
