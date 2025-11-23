<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <title>Articles</title>
    
    
    <link rel='stylesheet' type='text/css' href='style.css'>

</head>
<body>
    <main>
        <div class="content">
            <h1>Article list</h1>
            <hr>
            <div id="list">
                <p id="empty">There are no articles yet</p>
                <?php foreach ($data as $article){ ?>
                    
                <div class="article" id="<?php echo $article['id'];?>">
                    <p class="article_name"><?php echo $article['name_']; ?></p>
                    <div class = "article_buttons">
                        <a class="btn show" href="article/<?php echo $article['id']; ?>">Show</a>
                        <a class="btn edit" href="article-edit/<?php echo $article['id']; ?>">Edit</a>
                        <button class="btn delete" id="delete">Delete</button>
                    </div>
                </div>

                <?php } ?>
            </div>
            <hr>
            <div class="buttons">
                <button id="previous">Previous</button>
                <button id="next">Next</button>
                <p id="page_count"></p>
                <button id="dialogBtn">Create article</button>
            </div>
        </div>


        <dialog id="dialog">
            <p>Name</p>
            <form name="new_article_d" class="form">
                <input id="new_article_name" type="text" value="" maxlength="32" required>
            <p>Password</p>
                <input id="password_d" type="password"  value=""  maxlength="32" minlength="8" required>
                
                <div class="buttons_d">
                    <input  id="cancel" type="button" value="Cancel">
                    <input  id="create" type="button" value="Create">
                </div>
            </form>
        </dialog>

        <script type="text/javascript" src="articles.js"></script>

        </main>
</body>
</html>