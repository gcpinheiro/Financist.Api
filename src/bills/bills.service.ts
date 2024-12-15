import {
  BadRequestException,
  Injectable,
  InternalServerErrorException,
} from '@nestjs/common';
import { PrismaService } from 'src/prisma/prisma.service';
import { CreateBillDto } from './dto/create-bill.dto';

@Injectable()
export class BillsService {
  constructor(private prisma: PrismaService) {}

  async createBill(dto: CreateBillDto) {
    const {
      name,
      value,
      currentInstallment,
      totalInstallment,
      remainingValue,
      cardId,
      categoryId,
      subCategoryId,
      userId,
      accountType,
    } = dto;
    try {
      console.log('Creating bill with payload:', dto);
      return await this.prisma.bills.create({
        data: {
          name,
          value,
          currentInstallment,
          totalInstallment,
          remainingValue,
          cardId,
          categoryId,
          subCategoryId,
          userId,
          accountType,
        },
      });
    } catch (error) {
      console.error('Error details:', error); // Log completo do erro
      if (error.code === 'P2003') {
        // Prisma error: foreign key constraint failed
        throw new BadRequestException(
          'Invalid foreign key reference. Please check your IDs.',
        );
      } else if (error.code === 'P2002') {
        // Prisma error: unique constraint failed
        throw new BadRequestException('A unique constraint has been violated.');
      } else {
        // Unexpected errors
        throw new InternalServerErrorException(
          'An unexpected error occurred while creating the bill.',
        );
      }
    }
  }

  async getBills(userId: number) {
    const bills = await this.prisma.bills.findMany({
      where: { userId },
    });

    const totalValue = await this.prisma.bills.aggregate({
      _sum: {
        value: true,
      },
    });

    console.log('iaaaaaad: ', typeof userId);
    console.log('Bills: ', bills);

    if (bills) {
      return {
        total: totalValue._sum.value,
        bills,
      };
    }
  }

  // async getTotalBills(userId: number) {
  //   const bills = await this.prisma.bills.findMany({
  //     where: { userId },
  //   });

  //   let total = 0;
  //   bills.forEach((element) => {
  //     total = +element.value;
  //   });
  // }
}
