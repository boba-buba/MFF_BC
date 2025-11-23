<?php

class articles
{
	function __construct()
	{
		$this->db = new DB_Articles();
	}

	function default($id = null)
	{
		$data = $this->db->get_articles();	
		include __DIR__. '/../templates/articles.php';
	}
}