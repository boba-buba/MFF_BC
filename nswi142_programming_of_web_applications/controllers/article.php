<?php

class article
{
    function __construct()
	{
		$this->db = new DB_Articles();
        $this->host = 'http://'.$_SERVER['HTTP_HOST'].'/';
	}

	function default($id)
	{
        $data = $this->db->get_article($id);
        if (empty($data)){
            include __DIR__. '/../templates/error404.php';
        } else {
            include __DIR__. '/../templates/article.php';
        }
	}

    function edit($id){
        //$correct_password = $this->db->correct_pwd;
        $data = $this->db->get_article($id);
        $wrong_pswd = 'none';
        if (empty($data)){
            include __DIR__. '/../templates/error404.php';
        } else {
            include __DIR__. '/../templates/article-edit.php';
        }

    }

    function edit2($id, $content, $name){
        $data[0]['name_'] = $name;
        $data[0]['id'] = $id;
        $data[0]['content'] = $content;
        $wrong_pswd = 'block';
        include __DIR__. '/../templates/article-edit.php';
    }


    

    function create($name, $pwd){
        $id = $this->db->create_article($name, $pwd);
        header('Location:'.$this->host.'/~72707943/cms/article-edit/'.$id);
    }


    function save($id,  $content, $name, $pwd){
        $this->db->save_article($id,  $content, $name, $pwd);
        
        if ($this->db->correct_pwd === true){
            header('Location:'.$this->host.'/~72707943/cms/articles');
        }
        else{
            $this->edit2($id, $content, $name);
            //header('Location:'.$this->host.'/~72707943/cms/article-edit/'.$id);
        }
    }

    function delete($id)
	{
        $this->db->delete_article($id);
	}
}