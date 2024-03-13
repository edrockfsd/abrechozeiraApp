import { NbMenuItem } from '@nebular/theme';

export const MENU_ITEMS: NbMenuItem[] = [
  {
    title: 'Lives',
    icon: 'video-outline',
    link: '/pages/dashboard',
    home: true,
  },
  {
    title: 'Vendas',
    icon: 'shopping-bag-outline',
    link: '/vendas/listar',
    children:[
      {
        title: 'Listar',
        link: 'listar'
      },
      {
        title: 'Adicionar',
        link:'cadastro'
      },
      {
        title: 'Upload',
        link:'upload'
      }
    ],    
  },
  {
    title: 'Estoque',
    icon: 'cube-outline',
    link: '/pages/iot-dashboard',
  },
  {
    title: 'Clientes',
    icon: 'people-outline',    
    link: '/clientes',
  }
];
