<?php
require_once __DIR__ . '/db_connection.php';

class DB_Articles
{
    var $db_con;
    var $correct_pwd = false;
	public function __construct()
	{
		$this->db_con = new Connection();
	} 

	public function get_articles($id = null, $content = null, $name = null)
	{
        $this->db_con->execute_query('SELECT * FROM `articles`;');
        return $this->db_con->result;
	}

    public function get_article($id, $content = null, $name = null){
        $this->db_con->execute_query("SELECT * FROM `articles` WHERE id=${id};");
        //var_dump($this->db_con->result);
        return $this->db_con->result;
    }

    public function get_pwd($id){
        $this->db_con->execute_query("SELECT pwd FROM `articles` WHERE id=${id};");
        return $this->db_con->result;
    }

    public function save_article($id, $content, $name, $pwd){
        $this->correct_pwd = false;
        $pwd_db = $this->get_pwd($id);
        if ($pwd_db[0]["pwd"] ==  $pwd){
            $this->db_con->execute_query("UPDATE `articles` SET `content`='$content' WHERE id=$id;");
            $this->db_con->execute_query("UPDATE `articles` SET `name_`='$name' WHERE id=$id;");
            $this->correct_pwd = true;
        }        
    }

    public function create_article($name,  $pwd, $id = null, $content = null){
        $this->db_con->execute_query("SELECT MAX(id) FROM `articles`;");
        //var_dump($this->db_con->result);
        $id = $this->db_con->result[0]["MAX(id)"] + 1;

        $query = "INSERT INTO `articles` (`id`, `name_`, `content`, `pwd`) VALUES (${id},'${name}','', '${pwd}');";

        //echo $query;
        $this->db_con->execute_query($query);
        return $id;
    }

    public function delete_article($id, $content = null, $name = null, $pwd = null){      
        try {
            $this->db_con->execute_query("DELETE FROM `articles` WHERE id=$id;");
            echo json_encode(array('delete' => 'success'));
        }
        catch (Exception $e) {
            echo json_encode(array('delete' => 'failed'));
        }     
    }
}