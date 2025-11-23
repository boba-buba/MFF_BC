<?php

require_once __DIR__ . '/lib/db_connection.php';
$id = $_GET['del'];
$db_con = new Connection();

try{
    $db_con->execute_query("DELETE FROM `articles` WHERE id=$id;");
    echo json_encode(array('delete' => 'success'));
}
catch  (Exception $e) {
    echo json_encode(array('delete' => 'failed'));
}