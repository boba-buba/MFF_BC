function list_articles(){
    articles.forEach((article, index) => {
        if (index < (current_page * per_page) && index >= ((current_page - 1) * per_page)) {
            article.style.display = 'flex';
        } 
        else {
            article.style.display = 'none';
        }
    });
}

function next_page() {
    
    if (current_page < page_count_total) {
        current_page++;
        document.getElementById('page_count').innerText = current_page + ' of ' + page_count_total;
        list_articles(per_page, articles);
        document.getElementById('previous').style.display = "inline-block";
        if (current_page == page_count_total) document.getElementById('next').style.display = "none";
    } 
}

function previous_page() {
    if (current_page > 1) {
        current_page--;
        document.getElementById('page_count').innerText = current_page + ' of ' + page_count_total;
        list_articles(per_page, articles);
        document.getElementById('next').style.display = "inline-block";
        if (current_page == 1) document.getElementById('previous').style.display = "none";
    }
}

function create_article(){
    let new_name = document.getElementById('new_article_name').value;
    let new_pwd = document.getElementById('password_d').value;
    //new_pwd = password_hash(new_pwd, PASSWORD_DEFAULT); 
    if (new_name != '' && new_pwd.length >= 8) {
        window.location = 'https://webik.ms.mff.cuni.cz/~72707943/cms/?page=article-create&name=' + new_name+'&pwd='+new_pwd;
    }
}

 


document.getElementById('next').addEventListener('click', next_page);
document.getElementById('previous').addEventListener('click', previous_page);
document.getElementById('previous').style.display = "none";
document.getElementById('dialogBtn').addEventListener('click', function(){document.getElementById('dialog').showModal();});
document.getElementById('cancel').addEventListener('click',  function() {
    document.getElementById('new_article_name').value = "";
    document.getElementById('password_d').value = "";
    document.getElementById('dialog').close();

});
document.getElementById('create').addEventListener('click', create_article);

let delete_buttons = document.querySelectorAll('#delete');
delete_buttons.forEach((btn) => { btn.addEventListener('click', function (){
    const id = btn.parentElement.parentElement.id;
    fetch(
    "fetch-delete.php?del="+id,
    {method: "DELETE"})
    .then((response) => response.json())
    .then((data) => {
        console.log('Success:', data);
        articles_count-=1;
        page_count_total = Math.max(Math.ceil(document.getElementsByClassName('article').length / per_page), 1);

        
        document.getElementById('list').removeChild(document.getElementById(id));

        page_count_total = Math.ceil(document.getElementsByClassName('article').length / per_page);
        if (page_count_total < current_page) current_page = page_count_total;
        if (document.getElementsByClassName('article').length == 0) document.getElementById('empty').style.display = 'block';
        
        
        
        articles = document.querySelectorAll('.article');

        

        list_articles(per_page, articles);
        document.getElementById('page_count').innerText = current_page + ' of ' + page_count_total;
    })
    .catch((error) => {
        console.error('Error:', error);
    });

});});


const per_page = 10;

let current_page = 1;
let articles_array = document.getElementsByClassName('article')

let articles = [];
for (var i = articles_array.length - 1; i >= 0; i--) {
    articles.push(articles_array[i]);
  }


let articles_count = document.getElementsByClassName('article').length;
if (articles_count != 0){
    document.getElementById('empty').style.display = 'none';
}


let page_count_total = Math.max(Math.ceil(document.getElementsByClassName('article').length / per_page), 1);

list_articles(per_page, articles);
document.getElementById('page_count').innerText = current_page + ' of ' + page_count_total;
