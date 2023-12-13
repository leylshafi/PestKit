const cartItemHolder = document.querySelector(".cart-item-holder");
const addToCartButtons = document.querySelectorAll(".add-to-cart");
const removeButtons= document.querySelectorAll(".remove-from-cart");

removeButtons.forEach(button =>
    button.addEventListener("click", ev => {
        ev.preventDefault();
        console.log("test");

        const href = ev.target.getAttribute("href");

        console.log(cartItemHolder);

        fetch(href).then(res => res.text()).then(data => {
            cartItemHolder.innerHTML = data;
        })
    })
)
function deleteBasketItem(id, count) {
    $.ajax({
        url: '/Basket/DeleteBasket',
        type: 'POST',
        data: { id: id, count: count },
        dataType: 'json',
        success: function (result) {
            if (result.success) {
                updateBasketView(result.basket);
            } else {
                console.log('Deletion was not successful.');
            }
        },
        error: function () {
            console.error('An error occurred during the AJAX request.');
        }
    });
}

function updateBasketView(basketData) {
    $('#layoutBasketPartial').html(basketData);
    
    $('#mainPageBasketPartial').html(basketData);
}

addToCartButtons.forEach(button =>
    button.addEventListener("click", ev => {
        ev.preventDefault();
        console.log("test");

        const href = ev.target.getAttribute("href");

        console.log(cartItemHolder);

        fetch(href).then(res => res.text()).then(data => {
            cartItemHolder.innerHTML = data;
        })
    })
)
