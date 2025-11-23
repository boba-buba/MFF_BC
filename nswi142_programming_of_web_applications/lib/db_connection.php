<?php
//require_once __DIR__ . '/db_config.php';
class Connection 
{
	var $server;
	var $dblogin;
	var $dbpass;
	var $db;
	var $mysqli;
	var $query;
	var $result;
	var $data;
	
	function __construct() {
		require_once __DIR__ . '/db_config.php';
		$this->server = $db_config['server'];
		$this->dblogin = $db_config['login'];
		$this->dbpass = $db_config['password'];
		$this->db = $db_config['database'];
	}
	function connect() {
		

		//$this->mysqli = new mysqli($db_config['server'],  $db_config['login'], $db_config['password'], $db_config['database']);
		$this->mysqli = new mysqli($this->server, $this->dblogin, $this->dbpass, $this->db);
		if (!$this->mysqli) die("Unable to connect to the database.");
		mysqli_set_charset($this->mysqli, "utf8") || die("Seting MySQL charset failed");
	}

	function execute_query($query){
		$this->connect();
		$this->data = $this->mysqli->query($query);

		if (!is_bool($this->data)){
			$this->result = $this->data->fetch_all(MYSQLI_ASSOC);
		}
		$this->mysqli->close();
	}
}