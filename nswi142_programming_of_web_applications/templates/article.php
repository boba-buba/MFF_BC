<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <title>Articles</title>
    <link rel='stylesheet' type='text/css' href='../style.css'>

</head>
<body>
    <main>
        <script type="text/javascript">
            function edit(){
                window.location = 'https://webik.ms.mff.cuni.cz/~72707943/cms/article-edit/<?php echo $data[0]['id']?>'
            }
        </script>

        <div class="content">
            <h1><?php echo $data[0]['name_']?></h1>
            <p class="article_content"><?php echo $data[0]['content']?></p>
            <div class="buttons">
                <button class="btn a_edit" onclick="edit()">Edit</button>
                <button class="btn back" onclick='window.location = "https://webik.ms.mff.cuni.cz/~72707943/cms/articles"'>Back to articles</button>
            </div>
        </div>
    </main>
</body>
</html>