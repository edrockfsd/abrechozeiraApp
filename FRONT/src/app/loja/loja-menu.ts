import { NbMenuItem } from '@nebular/theme';

export const MENU_ITEMS: NbMenuItem[] = [
  // {
  //   title: 'Lives',
  //   icon: 'video-outline',
  //   link: '/pages/dashboard',
  //   home: true,
  // },
  // {
  //   title: 'Vendas',
  //   icon: 'shopping-bag-outline',
  //   link: '/vendas/listar',
  //   children:[
  //     {
  //       title: 'Listar',
  //       link: 'listar'
  //     },
  //     {
  //       title: 'Adicionar',
  //       link:'cadastro'
  //     },
  //     {
  //       title: 'Upload',
  //       link:'upload'
  //     }
  //   ],    
  // },
  // {
  //   title: 'Estoque',
  //   icon: 'cube-outline',
  //   link: '/pages/iot-dashboard',
  // },
  // {
  //   title: 'Clientes',
  //   icon: 'people-outline',    
  //   link: '/clientes/cadastro',
  //   children:[
  //     {
  //       title: 'Listar',
  //       link: 'listar'
  //     },
  //     {
  //       title: 'Adicionar',
  //       link:'cadastro'
  //     },
  //     {
  //       title: 'Upload',
  //       link:'upload'
  //     }
  //   ],   
  // }
  
  {
    title: 'Lives',
    icon: 'video-outline',
    link: '/pages/dashboard',
    home: true,
  },
  {
    title: 'Vendas',
    icon: 'shopping-bag-outline',
    link: '/vendas',
    children:[
      {
        title: 'Listar',
        link: '/vendas/listar'
      },
      {
        title: 'Adicionar',
        link:'/vendas/cadastro'
      },
      {
        title: 'Upload',
        link:'/vendas/upload'
      }
    ],    
  },
  {
    title: 'Clientes',
    icon: 'people-outline',
    link: '/clientes',
    children:[
      {
        title: 'Listar',
        link: '/clientes/listar'
      },
      {
        title: 'Adicionar',
        link:'/clientes/cadastro'
      },
      {
        title: 'Upload',
        link:'/clientes/upload'
      }
    ],   
  },
  {
    title: 'Estoque',
    icon: 'people-outline',
    link: '/estoque',
    children:[
      {
        title: 'Listar',
        link: '/estoque/listar'
      },
      {
        title: 'Cadastro',
        link:'/cadastro/cadastro'
      }
    ],   
  },
];
