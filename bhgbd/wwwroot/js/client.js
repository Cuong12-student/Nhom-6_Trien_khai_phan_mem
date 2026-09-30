(() => {
    const one = (selector, root = document) => root.querySelector(selector);
    const all = (selector, root = document) => [...root.querySelectorAll(selector)];
    const money = value => `${Math.max(0, Math.round(value)).toLocaleString('vi-VN')}đ`;
    let toastTimer;

    const toast = message => {
        const element = one('[data-site-toast]');
        if (!element) return;
        element.textContent = message;
        element.classList.add('open');
        window.clearTimeout(toastTimer);
        toastTimer = window.setTimeout(() => element.classList.remove('open'), 2600);
    };

    const initHeader = () => {
        const menuButton = one('[data-mobile-menu]');
        const nav = one('[data-main-nav]');
        const searchPanel = one('[data-search-panel]');
        menuButton?.addEventListener('click', () => {
            nav?.classList.toggle('open');
            document.body.classList.toggle('menu-open', nav?.classList.contains('open'));
        });
        one('[data-search-toggle]')?.addEventListener('click', () => {
            searchPanel?.classList.toggle('open');
            if (searchPanel?.classList.contains('open')) one('input', searchPanel)?.focus();
        });
        one('[data-search-close]')?.addEventListener('click', () => searchPanel?.classList.remove('open'));
        all('[data-main-nav] a').forEach(link => link.addEventListener('click', () => {
            nav?.classList.remove('open');
            document.body.classList.remove('menu-open');
        }));
    };

    const initHero = () => {
        const root = one('[data-hero-carousel]');
        if (!root) return;
        const slides = all('[data-hero-slide]', root);
        const dots = all('[data-hero-dot]', root);
        const video = one('[data-hero-video]', root);
        const button = one('[data-sound-toggle]', root);
        let current = 0;
        let timer;
        const show = index => {
            current = (index + slides.length) % slides.length;
            slides.forEach((slide, slideIndex) => slide.classList.toggle('active', slideIndex === current));
            dots.forEach((dot, dotIndex) => dot.classList.toggle('active', dotIndex === current));
            if (video) {
                if (current === 0) video.play().catch(() => {});
                else video.pause();
            }
        };
        const schedule = () => {
            window.clearInterval(timer);
            timer = window.setInterval(() => show(current + 1), 8000);
        };
        dots.forEach(dot => dot.addEventListener('click', () => {
            show(Number(dot.dataset.heroDot || 0));
            schedule();
        }));
        if (video && button) {
            const sync = () => {
                button.classList.toggle('active', !video.muted);
                const label = one('b', button);
                if (label) label.textContent = video.muted ? 'BẬT ÂM' : 'TẮT ÂM';
            };
            button.addEventListener('click', () => {
                video.muted = !video.muted;
                video.play().catch(() => {});
                sync();
            });
            sync();
        }
        show(0);
        schedule();
    };

    const initFavorites = () => {
        all('[data-favorite]').forEach(button => button.addEventListener('click', event => {
            event.preventDefault();
            event.stopPropagation();
            button.classList.toggle('active');
            button.textContent = button.classList.contains('active') ? '♥' : '♡';
            toast(button.classList.contains('active') ? 'Đã thêm vào sản phẩm yêu thích.' : 'Đã bỏ khỏi sản phẩm yêu thích.');
        }));
    };

    const initCatalog = () => {
        const catalog = one('[data-catalog]');
        if (!catalog) return;
        const cards = all('[data-product-card]', catalog);
        const grid = one('[data-product-grid]', catalog);
        const empty = one('[data-catalog-empty]', catalog);
        const count = one('[data-result-count]', catalog);
        const heroCount = one('[data-hero-result-count]');
        const title = one('[data-catalog-title]');
        const description = one('[data-catalog-description]');
        const search = one('[data-product-search]', catalog);
        const panel = one('[data-filter-panel]', catalog);
        const overlay = one('.filter-overlay', catalog);
        const initialSole = (catalog.dataset.initialSole || '').toUpperCase();
        const initialBrand = catalog.dataset.initialBrand || '';
        const initialSearch = catalog.dataset.initialSearch || '';
        let selectedSize = '';

        if (search && initialSearch) search.value = initialSearch;
        if (initialSole) {
            all('[data-filter-sole]', catalog).forEach(input => input.checked = input.value === initialSole);
        }
        if (initialBrand) {
            all('[data-filter-brand]', catalog).forEach(input => input.checked = input.value.toLowerCase() === initialBrand.toLowerCase());
        }

        const checkedValues = selector => all(`${selector}:checked`, catalog).map(input => input.value);
        const apply = () => {
            const keyword = search?.value.trim().toLowerCase() || '';
            const brands = checkedValues('[data-filter-brand]');
            const soles = checkedValues('[data-filter-sole]');
            const priceRange = one('[data-filter-price]:checked', catalog)?.value || 'all';
            let visible = 0;

            cards.forEach(card => {
                const name = card.dataset.name || '';
                const brand = card.dataset.brand || '';
                const sole = card.dataset.sole || '';
                const price = Number(card.dataset.price || 0);
                const sizes = (card.dataset.sizes || '').split(' ');
                const matchesKeyword = !keyword || name.includes(keyword) || brand.toLowerCase().includes(keyword) || sole.toLowerCase().includes(keyword);
                const matchesBrand = !brands.length || brands.includes(brand);
                const matchesSole = !soles.length || soles.includes(sole);
                const matchesSize = !selectedSize || sizes.includes(selectedSize);
                const matchesPrice = priceRange === 'all' || (priceRange === 'under-2' && price < 2000000) || (priceRange === '2-3' && price >= 2000000 && price <= 3000000) || (priceRange === 'over-3' && price > 3000000);
                const show = matchesKeyword && matchesBrand && matchesSole && matchesSize && matchesPrice;
                card.hidden = !show;
                if (show) visible += 1;
            });

            if (count) count.textContent = `${visible} sản phẩm`;
            if (heroCount) heroCount.textContent = String(visible);
            if (empty) empty.style.display = visible ? 'none' : 'block';
            if (grid) grid.style.display = visible ? 'grid' : 'none';

            if (title && description) {
                if (soles.length === 1 && soles[0] === 'TF') {
                    title.textContent = 'GIÀY TF';
                    description.textContent = 'Thiết kế cho sân cỏ nhân tạo với độ bám linh hoạt và phân bổ đinh ổn định.';
                } else if (soles.length === 1 && soles[0] === 'FG') {
                    title.textContent = 'GIÀY FG';
                    description.textContent = 'Thiết kế cho sân cỏ tự nhiên, ưu tiên tốc độ, lực bám và khả năng bứt phá.';
                } else if (soles.length === 1 && soles[0] === 'IC') {
                    title.textContent = 'GIÀY IC / FUTSAL';
                    description.textContent = 'Đế phẳng dành cho sân trong nhà và futsal, hỗ trợ kiểm soát bóng ở không gian hẹp.';
                } else if (soles.length === 1 && soles[0] === 'AG') {
                    title.textContent = 'GIÀY AG';
                    description.textContent = 'Cấu trúc đinh dành cho mặt sân cỏ nhân tạo chuẩn, cân bằng giữa độ bám và khả năng xoay trở.';
                } else {
                    title.textContent = 'GIÀY BÓNG ĐÁ';
                    description.textContent = 'Chọn theo mặt sân, thương hiệu, khoảng giá và size phù hợp.';
                }
            }
        };

        search?.addEventListener('input', apply);
        all('[data-filter-brand], [data-filter-sole], [data-filter-price]', catalog).forEach(input => input.addEventListener('change', apply));
        all('[data-filter-size]', catalog).forEach(button => button.addEventListener('click', () => {
            const value = button.dataset.filterSize || '';
            selectedSize = selectedSize === value ? '' : value;
            all('[data-filter-size]', catalog).forEach(item => item.classList.toggle('active', item.dataset.filterSize === selectedSize));
            apply();
        }));

        one('[data-sort-products]', catalog)?.addEventListener('change', event => {
            const mode = event.target.value;
            const sorted = [...cards].sort((a, b) => {
                const aPrice = Number(a.dataset.price || 0);
                const bPrice = Number(b.dataset.price || 0);
                if (mode === 'price-asc') return aPrice - bPrice;
                if (mode === 'price-desc') return bPrice - aPrice;
                if (mode === 'new') return Number(b.querySelector('.product-tag')?.textContent.includes('MỚI')) - Number(a.querySelector('.product-tag')?.textContent.includes('MỚI'));
                return 0;
            });
            sorted.forEach(card => grid?.appendChild(card));
        });

        all('[data-filter-heading]', catalog).forEach(button => button.addEventListener('click', () => {
            const options = button.nextElementSibling;
            const collapsed = options?.getAttribute('hidden') !== null;
            if (collapsed) options?.removeAttribute('hidden');
            else options?.setAttribute('hidden', '');
            const mark = one('span', button);
            if (mark) mark.textContent = collapsed ? '−' : '＋';
        }));

        one('[data-filter-open]', catalog)?.addEventListener('click', () => {
            panel?.classList.add('open');
            overlay?.classList.add('open');
            document.body.classList.add('modal-open');
        });
        all('[data-filter-close]', catalog).forEach(button => button.addEventListener('click', () => {
            panel?.classList.remove('open');
            overlay?.classList.remove('open');
            document.body.classList.remove('modal-open');
        }));
        apply();
    };

    const initProductDetail = () => {
        const root = one('[data-product-detail-ui]');
        if (!root) return;
        const mainImage = one('[data-main-image]', root);
        const mainWrap = one('[data-main-image-wrap]', root);
        const lens = one('[data-image-lens]', root);
        const sizeMessage = one('[data-size-message]', root);
        const quantity = one('[data-quantity-input]', root);
        let chosenSize = '';

        all('[data-gallery-thumb]', root).forEach(button => button.addEventListener('click', () => {
            if (!mainImage) return;
            all('[data-gallery-thumb]', root).forEach(item => item.classList.remove('active'));
            button.classList.add('active');
            mainImage.style.opacity = '.25';
            window.setTimeout(() => {
                mainImage.src = button.dataset.image || mainImage.src;
                mainImage.alt = button.dataset.alt || '';
                mainImage.style.opacity = '1';
            }, 120);
        }));

        if (mainWrap && mainImage && lens && matchMedia('(hover:hover) and (pointer:fine)').matches) {
            const moveLens = event => {
                const rect = mainWrap.getBoundingClientRect();
                const lensSize = lens.offsetWidth || 190;
                const zoom = 2.35;
                const x = Math.max(0, Math.min(event.clientX - rect.left, rect.width));
                const y = Math.max(0, Math.min(event.clientY - rect.top, rect.height));
                lens.style.left = `${x}px`;
                lens.style.top = `${y}px`;
                lens.style.backgroundImage = `url("${mainImage.currentSrc || mainImage.src}")`;
                lens.style.backgroundSize = `${rect.width * zoom}px ${rect.height * zoom}px`;
                lens.style.backgroundPosition = `${-(x * zoom - lensSize / 2)}px ${-(y * zoom - lensSize / 2)}px`;
            };
            mainWrap.addEventListener('mouseenter', event => {
                mainWrap.classList.add('zooming');
                moveLens(event);
            });
            mainWrap.addEventListener('mousemove', moveLens);
            mainWrap.addEventListener('mouseleave', () => mainWrap.classList.remove('zooming'));
        }

        all('[data-size]', root).forEach(button => button.addEventListener('click', () => {
            chosenSize = button.dataset.size || '';
            all('[data-size]', root).forEach(item => item.classList.toggle('active', item === button));
            if (sizeMessage) {
                sizeMessage.textContent = `Đã chọn size ${chosenSize}.`;
                sizeMessage.classList.remove('error');
            }
        }));

        all('[data-sole]', root).forEach(button => button.addEventListener('click', () => {
            all('[data-sole]', root).forEach(item => item.classList.toggle('active', item === button));
            const label = one('[data-sole-label]', root);
            if (label) label.textContent = button.dataset.sole || '';
        }));

        one('[data-quantity-minus]', root)?.addEventListener('click', () => {
            if (quantity) quantity.value = String(Math.max(1, Number(quantity.value || 1) - 1));
        });
        one('[data-quantity-plus]', root)?.addEventListener('click', () => {
            if (quantity) quantity.value = String(Math.min(9, Number(quantity.value || 1) + 1));
        });
        one('[data-scroll-size]', root)?.addEventListener('click', () => one('#size-guide')?.scrollIntoView({ behavior: 'smooth', block: 'start' }));
        one('[data-add-detail]', root)?.addEventListener('click', () => {
            if (!chosenSize) {
                if (sizeMessage) {
                    sizeMessage.textContent = 'Vui lòng chọn size trước khi thêm vào giỏ.';
                    sizeMessage.classList.add('error');
                }
                one('[data-size-options]', root)?.scrollIntoView({ behavior: 'smooth', block: 'center' });
                return;
            }
            toast(`Đã thêm size ${chosenSize} vào giỏ hàng.`);
            const count = one('[data-cart-count]');
            if (count) count.textContent = String(Number(count.textContent || 0) + Number(quantity?.value || 1));
        });
    };

    const initCart = () => {
        const root = one('[data-cart-ui]');
        if (!root) return;
        let discountRate = 0;
        const update = () => {
            const items = all('[data-cart-item]', root).filter(item => !item.hidden);
            let subtotal = 0;
            items.forEach(item => {
                const price = Number(item.dataset.unitPrice || 0);
                const input = one('[data-cart-quantity]', item);
                const amount = Math.max(1, Number(input?.value || 1));
                const total = price * amount;
                subtotal += total;
                const label = one('[data-item-total]', item);
                if (label) label.textContent = money(total);
            });
            const discount = Math.round(subtotal * discountRate);
            const subtotalLabel = one('[data-cart-subtotal]', root);
            const discountLabel = one('[data-cart-discount]', root);
            const totalLabel = one('[data-cart-grand-total]', root);
            if (subtotalLabel) subtotalLabel.textContent = money(subtotal);
            if (discountLabel) discountLabel.textContent = discount ? `-${money(discount)}` : '0đ';
            if (totalLabel) totalLabel.textContent = money(subtotal - discount);
            const empty = one('[data-cart-empty]', root);
            if (empty) empty.style.display = items.length ? 'none' : 'block';
            const count = one('[data-cart-count]');
            if (count) count.textContent = String(items.reduce((sum, item) => sum + Number(one('[data-cart-quantity]', item)?.value || 1), 0));
        };

        all('[data-cart-item]', root).forEach(item => {
            const input = one('[data-cart-quantity]', item);
            one('[data-cart-minus]', item)?.addEventListener('click', () => {
                if (input) input.value = String(Math.max(1, Number(input.value || 1) - 1));
                update();
            });
            one('[data-cart-plus]', item)?.addEventListener('click', () => {
                if (input) input.value = String(Math.min(9, Number(input.value || 1) + 1));
                update();
            });
            input?.addEventListener('change', update);
            one('[data-remove-item]', item)?.addEventListener('click', () => {
                item.hidden = true;
                update();
                toast('Đã xóa sản phẩm khỏi giỏ hàng.');
            });
        });

        one('[data-voucher-form]', root)?.addEventListener('submit', event => {
            event.preventDefault();
            const code = one('[data-voucher-code]', root)?.value.trim().toUpperCase();
            const message = one('[data-voucher-message]', root);
            if (code === 'BHGBD10') {
                discountRate = .1;
                if (message) {
                    message.textContent = 'Áp dụng thành công: giảm 10%.';
                    message.classList.add('success');
                }
            } else {
                discountRate = 0;
                if (message) {
                    message.textContent = code ? 'Mã voucher không hợp lệ.' : 'Vui lòng nhập mã voucher.';
                    message.classList.remove('success');
                }
            }
            update();
        });
        update();
    };

    const initCheckout = () => {
        const form = one('[data-checkout-form]');
        if (!form) return;
        const modal = one('[data-order-modal]');
        const bankDetails = one('[data-bank-details]', form);

        all('input[name="paymentMethod"]', form).forEach(input => input.addEventListener('change', () => {
            all('.payment-card', form).forEach(card => card.classList.toggle('active', one('input', card)?.checked));
            bankDetails?.classList.toggle('open', input.value === 'BANK' && input.checked);
        }));

        const validateField = field => {
            const wrapper = field.closest('.form-field');
            const error = one('[data-error]', wrapper);
            let message = '';
            if (field.hasAttribute('data-required') && !field.value.trim()) message = 'Vui lòng nhập thông tin bắt buộc.';
            if (!message && field.hasAttribute('data-phone') && !/^(0|\+84)[0-9\s.-]{8,12}$/.test(field.value.trim())) message = 'Số điện thoại chưa đúng định dạng.';
            wrapper?.classList.toggle('invalid', Boolean(message));
            if (error) error.textContent = message;
            return !message;
        };

        all('[data-required], [data-phone]', form).forEach(field => field.addEventListener('input', () => validateField(field)));
        form.addEventListener('submit', event => {
            event.preventDefault();
            const fields = all('[data-required], [data-phone]', form);
            const firstInvalid = fields.find(field => !validateField(field));
            if (firstInvalid) {
                firstInvalid.scrollIntoView({ behavior: 'smooth', block: 'center' });
                window.setTimeout(() => firstInvalid.focus(), 350);
                return;
            }
            if (!one('[data-agreement]', form)?.checked) {
                one('[data-agreement]', form)?.scrollIntoView({ behavior: 'smooth', block: 'center' });
                toast('Vui lòng xác nhận thông tin và chính sách đặt hàng.');
                return;
            }
            modal?.classList.add('open');
            document.body.classList.add('modal-open');
        });
        one('[data-modal-close]')?.addEventListener('click', () => {
            modal?.classList.remove('open');
            document.body.classList.remove('modal-open');
        });
        modal?.addEventListener('click', event => {
            if (event.target === modal) {
                modal.classList.remove('open');
                document.body.classList.remove('modal-open');
            }
        });
    };

    const initHistory = () => {
        const root = one('[data-order-history]');
        if (!root) return;
        const cards = all('[data-order-status]', root);
        const empty = one('[data-history-empty]', root);
        all('[data-order-tab]', root).forEach(button => button.addEventListener('click', () => {
            const status = button.dataset.orderTab;
            all('[data-order-tab]', root).forEach(item => item.classList.toggle('active', item === button));
            let visible = 0;
            cards.forEach(card => {
                const show = status === 'all' || card.dataset.orderStatus === status;
                card.hidden = !show;
                if (show) visible += 1;
            });
            if (empty) empty.style.display = visible ? 'none' : 'block';
        }));

        const modal = one('[data-order-detail-modal]');
        all('[data-order-detail]', root).forEach(button => button.addEventListener('click', () => {
            const code = one('[data-order-modal-code]', modal);
            if (code) code.textContent = button.dataset.orderCode || '';
            modal?.classList.add('open');
            document.body.classList.add('modal-open');
        }));
        one('[data-order-modal-close]')?.addEventListener('click', () => {
            modal?.classList.remove('open');
            document.body.classList.remove('modal-open');
        });
        modal?.addEventListener('click', event => {
            if (event.target === modal) {
                modal.classList.remove('open');
                document.body.classList.remove('modal-open');
            }
        });
        all('[data-cancel-order]', root).forEach(button => button.addEventListener('click', () => {
            const card = button.closest('[data-order-status]');
            if (!card) return;
            card.dataset.orderStatus = 'Cancelled';
            const badge = one('.status-badge', card);
            if (badge) {
                badge.textContent = 'YÊU CẦU HỦY';
                badge.className = 'status-badge status-badge--cancelled';
            }
            button.remove();
            toast('Đã chuyển đơn sang trạng thái yêu cầu hủy.');
        }));
    };

    document.addEventListener('DOMContentLoaded', () => {
        initHeader();
        initHero();
        initFavorites();
        initCatalog();
        initProductDetail();
        initCart();
        initCheckout();
        initHistory();
    });
})();
