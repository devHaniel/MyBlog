(() => {
    const root = document.querySelector(".editorial-home");
    if (!root) {
        return;
    }

    const searchInput = root.querySelector("#home-search-input");
    const status = root.querySelector(".home-search__status");
    const categoryButtons = [...root.querySelectorAll("[data-category-filter]")];
    let activeCategory = "all";

    const updateResults = () => {
        const query = searchInput?.value.trim().toLocaleLowerCase("es") ?? "";
        const articles = [...root.querySelectorAll("[data-home-article]")];
        const visibleIds = new Set();

        for (const article of articles) {
            const categories = (article.dataset.categoryIds ?? "").split(",");
            const matchesCategory = activeCategory === "all" || categories.includes(activeCategory);
            const matchesQuery = !query || article.textContent.toLocaleLowerCase("es").includes(query);
            const visible = matchesCategory && matchesQuery;
            article.hidden = !visible;
            if (visible) {
                visibleIds.add(article.dataset.postId);
            }
        }

        for (const section of root.querySelectorAll("[data-category-section]")) {
            section.hidden = !section.querySelector("[data-home-article]:not([hidden])");
        }

        if (status) {
            status.textContent = query || activeCategory !== "all"
                ? `${visibleIds.size} ${visibleIds.size === 1 ? "artículo encontrado" : "artículos encontrados"}.`
                : "";
        }
    };

    for (const button of categoryButtons) {
        button.addEventListener("click", () => {
            activeCategory = button.dataset.categoryFilter ?? "all";
            for (const option of categoryButtons) {
                const isActive = option === button;
                option.classList.toggle("is-active", isActive);
                option.setAttribute("aria-pressed", String(isActive));
            }
            updateResults();
        });
    }

    searchInput?.addEventListener("input", updateResults);

    const loadMoreButton = root.querySelector("[data-load-more]");
    const morePosts = root.querySelector("[data-more-posts]");
    loadMoreButton?.addEventListener("click", () => {
        if (!morePosts) {
            return;
        }

        morePosts.hidden = false;
        const additionalCards = [...morePosts.querySelectorAll("[data-home-article]")];
        for (const card of additionalCards) {
            const postId = card.dataset.postId;
            for (const duplicate of root.querySelectorAll("[data-home-article]")) {
                if (duplicate !== card && duplicate.dataset.postId === postId) {
                    duplicate.hidden = true;
                }
            }
        }

        loadMoreButton.hidden = true;
        updateResults();
    });

    updateResults();
})();
