<?php

class Router{

    public function dispatch()
    {
        try { 
            //
            //var_dump($_GET['page']); echo " get_page <br> ";
            //
            $url = explode('/', $_GET['page']);
            $templates = explode('-', $url[0]);

            $controller_name = empty($templates[0]) ? null : $templates[0];
            $action = empty($templates[1]) ? 'default' : $templates[1];

            include(__DIR__ . '/db_articles.php');
            include(__DIR__ . '/../controllers/' . $controller_name . '.php');
            
            $controller = new $controller_name;

            if ($controller_name != null) {
                
                    if (isset($_GET["name"])){
                        $controller->$action($_GET["name"], hash('ripemd160', $_GET["pwd"]));
                    }
                    else {
                        if (!empty($url[1]) && is_numeric($url[1])){

                            $id = (int) $url[1];
                            if (isset($_GET["content"])){                    
                                $controller->$action($id, $_GET["content"], $_GET["name_"], hash('ripemd160', $_GET["pwd"]));
                            }
                            else {
                                $controller->$action($id);
                            }
                        }
                        else {
                            $controller->$action();
                        }
                    }
                
            } else {
                throw new Exception('Controller was not found');
            }

        } catch (Exception $e) {

            header('HTTP/1.1 404 Not Found');
            header("Status: 404 Not Found");
            header('Location:http://'.$_SERVER['HTTP_HOST'].'/~72707943/cms/error404');


        }

    }
}