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
            function save(){
                let name = document.getElementById('edit_name').value;
                let pwd_to_compare = document.getElementById('password_d').value;
                console.log(name);
                if (name != ''){
                    let content = document.getElementById('edit_article').value.replace(/\n/g, '<br/>');
                    window.location = 'https://webik.ms.mff.cuni.cz/~72707943/cms/article-save/<?php echo $data[0]['id']?>?content=' + content + '&name_=' + name+ '&pwd=' + pwd_to_compare;
                }
            }


            
        </script>

        <div class="content">
            <form action="">
                <h1>Name</h1>
                <input required maxlength="32" type="text" id="edit_name" value="<?php echo $data[0]['name_']?>">
                <h1>Content</h1>
                <textarea id="edit_article" maxlength="1024"><?php echo $data[0]['content']?></textarea>
                <script type="text/javascript">
                    document.getElementById('edit_article').value = document.getElementById('edit_article').value.replace(/<br\/>/g, '\n');
                </script>
            </form>
            <p>Password</p>
            <input id="password_d" type="password"  value=""  maxlength="32" minlength="8" required>
            <p id="bad_password">bad password</p>
            <div class="buttons">
                <button class="btn save"  onclick="save()">Save</button>
                <button class="btn back" onclick="window.location = 'https://webik.ms.mff.cuni.cz/~72707943/cms/articles'">Back to articles</button>
            </div>  
        </div>
    </main>
    <script type="text/javascript">

            document.getElementById('bad_password').style.display = '<?php echo $wrong_pswd;?>';     
    </script>
</body>
</html>