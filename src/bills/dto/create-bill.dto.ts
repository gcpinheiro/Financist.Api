import {
  IsInt,
  IsOptional,
  IsPositive,
  IsString,
  IsNumber,
  IsNotEmpty,
} from 'class-validator';

export class CreateBillDto {
  @IsString()
  @IsNotEmpty()
  name: string;

  @IsNumber()
  @IsPositive()
  value: number;

  @IsOptional()
  @IsString()
  accountType: string;

  @IsOptional()
  @IsInt()
  currentInstallment?: number;

  @IsOptional()
  @IsInt()
  totalInstallment?: number;

  @IsOptional()
  @IsNumber()
  @IsPositive()
  remainingValue?: number;

  @IsInt()
  cardId: number;

  @IsInt()
  categoryId: number;

  @IsInt()
  subCategoryId: number;

  @IsInt()
  userId: number;
}
